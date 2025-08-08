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

        [HttpPost]
        public async Task<IActionResult> UploadClipboard([FromBody] DTO_UploadClipboardRequest request)
        {
            try
            {
                var userId = GetUserId();
                var clipboardEntryDto = await _clipboardService.UploadClipboardEntryAsync(userId, request);

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

        [HttpPost("restore/{entryId}")]
        public async Task<IActionResult> RestoreClipboardEntry(Guid entryId)
        {
            var userId = GetUserId();
            var restoredDto = await _clipboardService.RestoreClipboardEntryAsync(userId, entryId);
            // Notify clients of the newly restored (and now latest) item
            await _hubContext.Clients.Group(userId.ToString()).SendAsync("ReceiveClipboardUpdate", restoredDto);
            return Ok(restoredDto);
        }

        [HttpPut("{entryId}/pin")]
        public async Task<IActionResult> PinEntry(Guid entryId)
        {
            var dto = await _clipboardService.SetPinStatusAsync(GetUserId(), entryId, true);
            return Ok(dto);
        }
        [HttpPut("{entryId}/unpin")]
        public async Task<IActionResult> UnpinEntry(Guid entryId)
        {
            var dto = await _clipboardService.SetPinStatusAsync(GetUserId(), entryId, false);
            return Ok(dto);
        }

        [HttpPut("{entryId}/archive")]
        public async Task<IActionResult> ArchiveEntry(Guid entryId)
        {
            var dto = await _clipboardService.SetArchiveStatusAsync(GetUserId(), entryId, true);
            return Ok(dto);
        }

        [HttpPut("{entryId}/unarchive")]
        public async Task<IActionResult> UnarchiveEntry(Guid entryId)
        {
            var dto = await _clipboardService.SetArchiveStatusAsync(GetUserId(), entryId, false);
            return Ok(dto);
        }

        [HttpPut("{entryId}/tags")]
        public async Task<IActionResult> UpdateTags(Guid entryId, [FromBody] DTO_UpdateTags request)
        {
            var dto = await _clipboardService.UpdateTagsAsync(GetUserId(), entryId, request.Tags);
            return Ok(dto);
        }

        //Device Management Endpoints

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
