using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CopyHere.Application.Common.Settings;
using CopyHere.Application.DTO.Auth;
using CopyHere.Application.DTO.RefreshToken;
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
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(
           UserManager<User> userManager,
           SignInManager<User> signInManager,
           IJwtService jwtService,
          IOptions<ApplicationSettings> appSettings,
          IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _appSettings = appSettings.Value;
            _unitOfWork = unitOfWork;

        }

        public async Task<(bool Success, string? Message)> RegisterAsync (DTO_RegisterRequest request)
        {
            var user = new User
            {
                Email = request.Email,
                UserName = request.Email, // Use email as username for simplicity
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return (false, "User registration failed.");
            }
            // In a real app, you'd handle specific errors from result.Errors
            return (true, "User registered successfully.");
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
                var refreshToken = _jwtService.GenerateRefreshToken();

                var newRefreshToken = new RefreshToken
                {
                    Token = refreshToken,
                    UserId = user.Id,
                    Created = DateTime.UtcNow,
                    Expires = DateTime.UtcNow.AddDays(_appSettings.RefreshTokenExpiryDays)
                };

                await _unitOfWork.RefreshTokens.AddAsync(newRefreshToken);
                await _unitOfWork.CompleteAsync();

                return new DTO_LoginResponse
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    UserId = user.Id.ToString(),
                    Email = user.Email
                };
            }
            return null;
        }

        public async Task<DTO_LoginResponse?> RefreshTokenAsync (DTO_RefreshTokenRequest request)
        { var principal = _jwtService.GetPrincipalFromToken(request.Token);
            if(principal == null)
            {
                return null;
            }

            var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return null;
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return null;
            }

            var savedRefreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(request.RefreshToken);
            if (savedRefreshToken == null || savedRefreshToken.UserId != userId || !savedRefreshToken.IsActive)
            {
                return null;
            }

            savedRefreshToken.Revoked = DateTime.UtcNow;
            await _unitOfWork.RefreshTokens.UpdateAsync(savedRefreshToken);

            // Generate new tokens
            var newJwtToken = _jwtService.GenerateToken(user);
            var newRefreshTokenString = _jwtService.GenerateRefreshToken();

            var newRefreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = newRefreshTokenString,
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(_appSettings.RefreshTokenExpiryDays)
            };

            await _unitOfWork.RefreshTokens.AddAsync(newRefreshToken);
            await _unitOfWork.CompleteAsync();

            return new DTO_LoginResponse
            {
                Token = newJwtToken,
                RefreshToken = newRefreshTokenString,
                UserId = user.Id.ToString(),
                Email = user.Email
            };

        }

        public async Task<bool> RevokeTokenAsync(DTO_RevokeRefreshToken request)
        {
            var refreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(request.RefreshToken);
            if (refreshToken == null || !refreshToken.IsActive)
            {
                return false;
            }

            refreshToken.Revoked = DateTime.UtcNow;
            await _unitOfWork.RefreshTokens.UpdateAsync(refreshToken);
            await _unitOfWork.CompleteAsync();

            return true;
        }


    }
}
