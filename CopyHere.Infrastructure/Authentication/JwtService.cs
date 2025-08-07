using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CopyHere.Application.Common.Settings;
using CopyHere.Core.Entity;
using CopyHere.Core.Interfaces.IServices;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CopyHere.Infrastructure.Authentication
{
    public class JwtService : IJwtService
    {
        private readonly ApplicationSettings _appSettings;
        private readonly byte[] _key;

        public JwtService(IOptions<ApplicationSettings> appSettings)
        {
            _appSettings = appSettings.Value;
            _key = Encoding.UTF8.GetBytes(_appSettings.JwtSecret);
        }
        public string GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                    new Claim(ClaimTypes.Name, user.UserName ?? string.Empty)
                    // Add other claims as needed, e.g., roles
                }),
                Expires = DateTime.UtcNow.AddMinutes(_appSettings.JwtTokenExpiryMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public ClaimsPrincipal? GetPrincipalFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(_key),
                    ValidateIssuer = false, // For development, set to true in production and configure valid issuer
                    ValidateAudience = false, // For development, set to true in production and configure valid audience
                    ClockSkew = TimeSpan.Zero // No leeway for token expiry
                }, out SecurityToken validatedToken);

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
