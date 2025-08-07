using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.AspNetCore;
using CopyHere.Application.Common.Settings;
using CopyHere.Application.Interfaces.Services;
using CopyHere.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CopyHere.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            // Register application services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IClipboardService, ClipboardService>();

            // Configure application settings
            services.Configure<ApplicationSettings>(configuration.GetSection("ApplicationSettings"));

            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

            return services;
        }
    }
}
