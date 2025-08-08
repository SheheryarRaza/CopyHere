using CopyHere.Application.DTO.Auth;
using CopyHere.Application.DTO.RefreshToken;
using CopyHere.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CopyHere.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("fixed-window-policy")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] DTO_RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var (success, message) = await _authService.RegisterAsync(request);
            if (!success)
            {
                return BadRequest(new { message });
            }
            return Ok(new { message });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] DTO_LoginRequest request)
        {
            var response = await _authService.LoginAsync(request);
            if (response == null)
            {
                return Unauthorized(new { message = "Invalid credentials." });
            }
            return Ok(response);
        }
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] DTO_RefreshTokenRequest request)
        {
            var response = await _authService.RefreshTokenAsync(request);
            if (response == null)
            {
                return BadRequest(new { message = "Invalid token or refresh token." });
            }
            return Ok(response);
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke([FromBody] DTO_RevokeRefreshToken request)
        {
            var success = await _authService.RevokeTokenAsync(request);
            if (!success)
            {
                return BadRequest(new { message = "Invalid refresh token." });
            }
            return NoContent();
        }
    }
}
