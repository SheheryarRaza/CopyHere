using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CopyHere.Application.DTO.Auth;

namespace CopyHere.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<DTO_LoginResponse?> RegisterAsync(DTO_RegisterRequest request);
        Task<DTO_LoginResponse?> LoginAsync(DTO_LoginRequest request);
    }
}
