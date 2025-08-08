using System.Text;
using System.Threading.RateLimiting;
using CopyHere.Api.Hubs;
using CopyHere.Application; // For AddApplication extension method
using CopyHere.Application.Common.Settings; // To access ApplicationSettings
using CopyHere.Infrastructure; // For AddInfrastructure extension method
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models; // For Swagger/OpenAPI

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Fix for tools that require a top-level OpenAPI version field.
    c.SwaggerDoc("3.0.0", new OpenApiInfo { Title = "CopyHere API", Version = "3.0.0" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add Application and Infrastructure services using extension methods
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

// Configure JWT Authentication
var applicationSettings = builder.Configuration.GetSection("ApplicationSettings").Get<ApplicationSettings>();
if (applicationSettings == null || string.IsNullOrEmpty(applicationSettings.JwtSecret))
{
    throw new InvalidOperationException("ApplicationSettings:JwtSecret is not configured.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(applicationSettings.JwtSecret)),
        ValidateIssuer = false, // Set to true in production and configure valid issuer
        ValidateAudience = false, // Set to true in production and configure valid audience
        RequireExpirationTime = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // No leeway for token expiry
    };

    // Add debugging events for JWT authentication
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"JWT Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine($"JWT Token validated successfully for user: {context.Principal?.Identity?.Name}");
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            // If the request is for our hub...
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/clipboardHub")))
            {
                // Read the token out of the query string
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed-window-policy", fixedWindowOptions =>
    {
        fixedWindowOptions.PermitLimit = 5; // Allow 5 requests
        fixedWindowOptions.Window = TimeSpan.FromSeconds(10); // per 10 seconds
        fixedWindowOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        fixedWindowOptions.QueueLimit = 2; // Queue up to 2 requests
    });
});
builder.Services.AddProblemDetails();

// Add SignalR
builder.Services.AddSignalR();

// Add CORS policy for development (adjust for production)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("http://localhost:3000", "http://localhost:8080", "http://127.0.0.1:5500", "http://127.0.0.1:5501", "null") // Replace with your client app origins
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()); // Required for SignalR
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        // We are explicitly telling SwaggerUI to only use the 3.0.0 endpoint.
        // This prevents it from trying to fetch the default v1 endpoint, which
        // is the source of the 404 error.
        c.SwaggerEndpoint("/swagger/3.0.0/swagger.json", "CopyHere API v3.0.0");
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowSpecificOrigin"); // Use the CORS policy

app.UseRateLimiter();

app.UseAuthentication(); // Must be before UseAuthorization
app.UseAuthorization();

app.MapControllers();
app.MapHub<ClipboardHub>("/clipboardHub"); // Map SignalR Hub

app.Run();