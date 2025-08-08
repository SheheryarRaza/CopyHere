using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CopyHere.Core.Entity;

namespace CopyHere.Core.Interfaces.IServices
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromToken(string token);
    }
}
