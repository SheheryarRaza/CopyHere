using System.Security.Claims;
using CopyHere.Api.Hubs;
using CopyHere.Application.DTO.Clipboard;
using CopyHere.Application.DTO.Device;
using CopyHere.Application.Interfaces.Services;
using CopyHere.Core.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CopyHere.Api.Controllers
{
    [Authorize] // All actions in this controller require authentication
    [ApiController]
    [Route("api/[controller]")]
    public class ClipboardController : ControllerBase
    {
        private readonly IClipboardService _clipboardService;
        private readonly IHubContext<ClipboardHub> _hubContext;

        public ClipboardController(IClipboardService clipboardService, IHubContext<ClipboardHub> hubContext)
        {
            _clipboardService = clipboardService;
            _hubContext = hubContext;
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token.");
            }
            return userId;
        }

        /// <summary>
        /// Uploads a new clipboard entry.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UploadClipboard([FromBody] DTO_UploadClipboardRequest request)
        {
            try
            {
                var userId = GetUserId();
                var clipboardEntryDto = await _clipboardService.UploadClipboardEntryAsync(userId, request);

                // Notify all other connected devices of this user via SignalR
                // Exclude the originating device if you wish, but for clipboard sync,
                // usually all devices get the latest copy.
                await _hubContext.Clients.Group(userId.ToString()).SendAsync("ReceiveClipboardUpdate", clipboardEntryDto);

                return Ok(clipboardEntryDto);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, new { message = "An error occurred while uploading clipboard entry.", error = ex.Message });
            }
        }

        /// <summary>
        /// Gets the latest clipboard entry for the current user.
        /// </summary>
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestClipboard()
        {
            try
            {
                var userId = GetUserId();
                var entry = await _clipboardService.GetLatestClipboardEntryAsync(userId);
                if (entry == null)
                {
                    return NotFound(new { message = "No clipboard entries found." });
                }
                return Ok(entry);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, new { message = "An error occurred while retrieving latest clipboard entry.", error = ex.Message });
            }
        }

        /// <summary>
        /// Gets clipboard history for the current user.
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetClipboardHistory([FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            try
            {
                var userId = GetUserId();
                var history = await _clipboardService.GetClipboardHistoryAsync(userId, skip, take);
                return Ok(history);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, new { message = "An error occurred while retrieving clipboard history.", error = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a specific clipboard entry for the current user.
        /// </summary>
        [HttpDelete("{entryId}")]
        public async Task<IActionResult> DeleteClipboardEntry(Guid entryId)
        {
            try
            {
                var userId = GetUserId();
                await _clipboardService.DeleteClipboardEntryAsync(userId, entryId);
                return NoContent(); // 204 No Content
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, new { message = "An error occurred while deleting clipboard entry.", error = ex.Message });
            }
        }

        /// <summary>
        /// Clears all clipboard entries for the current user.
        /// </summary>
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearAllClipboardEntries()
        {
            try
            {
                var userId = GetUserId();
                await _clipboardService.ClearAllClipboardEntriesAsync(userId);
                return NoContent(); // 204 No Content
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, new { message = "An error occurred while clearing clipboard entries.", error = ex.Message });
            }
        }

        /// <summary>
        /// Registers a new device for the current user.
        /// </summary>
        [HttpPost("devices/register")]
        public async Task<IActionResult> RegisterDevice([FromBody] DTO_RegisterDeviceRequest request)
        {
            try
            {
                var userId = GetUserId();
                var deviceDto = await _clipboardService.RegisterDeviceAsync(userId, request);
                return Ok(deviceDto);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, new { message = "An error occurred while registering device.", error = ex.Message });
            }
        }

        /// <summary>
        /// Gets all registered devices for the current user.
        /// </summary>
        [HttpGet("devices")]
        public async Task<IActionResult> GetUserDevices()
        {
            try
            {
                var userId = GetUserId();
                var devices = await _clipboardService.GetUserDevicesAsync(userId);
                return Ok(devices);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, new { message = "An error occurred while retrieving user devices.", error = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a specific device for the current user.
        /// </summary>
        [HttpDelete("devices/{deviceId}")]
        public async Task<IActionResult> DeleteDevice(Guid deviceId)
        {
            try
            {
                var userId = GetUserId();
                await _clipboardService.DeleteDeviceAsync(userId, deviceId);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, new { message = "An error occurred while deleting device.", error = ex.Message });
            }
        }
    }
}
