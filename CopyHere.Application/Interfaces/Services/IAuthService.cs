using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CopyHere.Application.DTO.Auth;
using CopyHere.Application.DTO.RefreshToken;

namespace CopyHere.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string? Message)> RegisterAsync(DTO_RegisterRequest request);
        Task<DTO_LoginResponse?> LoginAsync(DTO_LoginRequest request);
        Task<DTO_LoginResponse?> RefreshTokenAsync(DTO_RefreshTokenRequest request);
        Task<bool> RevokeTokenAsync(DTO_RevokeRefreshToken request);
    }
}
