using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CopyHere.Application.DTO.Clipboard;
using CopyHere.Application.DTO.Device;

namespace CopyHere.Application.Interfaces.Services
{
    public interface IClipboardService
    {
        Task<DTO_ClipboardEntry> UploadClipboardEntryAsync(Guid userId, DTO_UploadClipboardRequest request);
        Task<DTO_ClipboardEntry?> GetLatestClipboardEntryAsync(Guid userId);
        Task<IEnumerable<DTO_ClipboardEntry>> GetClipboardHistoryAsync(Guid userId, int skip = 0, int take = 10, bool includeArchived = false);
        Task DeleteClipboardEntryAsync(Guid userId, Guid entryId);
        Task ClearAllClipboardEntriesAsync(Guid userId);
        Task<DTO_Device> RegisterDeviceAsync(Guid userId, DTO_RegisterDeviceRequest request);
        Task<IEnumerable<DTO_Device>> GetUserDevicesAsync(Guid userId);
        Task DeleteDeviceAsync(Guid userId, Guid deviceId);

        Task<DTO_ClipboardEntry> RestoreClipboardEntryAsync(Guid userId, Guid entryId);
        Task<DTO_ClipboardEntry> SetPinStatusAsync(Guid userId, Guid entryId, bool isPinned);
        Task<DTO_ClipboardEntry> SetArchiveStatusAsync(Guid userId, Guid entryId, bool isArchived);
        Task<DTO_ClipboardEntry> UpdateTagsAsync(Guid userId, Guid entryId, List<string> tags);
    }
}
