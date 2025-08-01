using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CopyHere.Application.Common.Settings;
using CopyHere.Application.DTO.Auth;
using CopyHere.Application.Interfaces.Services;
using CopyHere.Core.Entity;
using CopyHere.Core.Interfaces.IRepositories;
using CopyHere.Core.Interfaces.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace CopyHere.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly ApplicationSettings _appSettings;

        public AuthService(
           UserManager<User> userManager,
           SignInManager<User> signInManager,
           IJwtService jwtService,
          IOptions<ApplicationSettings> appSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _appSettings = appSettings.Value;

        }

        public async Task<DTO_LoginResponse?> RegisterAsync (DTO_RegisterRequest request)
        {
            var user = new User
            {
                Email = request.Email,
                UserName = request.Email, // Use email as username for simplicity
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                var token = _jwtService.GenerateToken(user);
                return new DTO_LoginResponse
                {
                    Token = token,
                    UserId = user.Id.ToString(),
                    Email = user.Email
                };
            }
            // In a real app, you'd handle specific errors from result.Errors
            return null;
        }

        public async Task<DTO_LoginResponse> LoginAsync (DTO_LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return null; // User not found
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

            if (result.Succeeded)
            {
                var token = _jwtService.GenerateToken(user);
                return new DTO_LoginResponse
                {
                    Token = token,
                    UserId = user.Id.ToString(),
                    Email = user.Email
                };
            }
            return null;
        }


    }
}
