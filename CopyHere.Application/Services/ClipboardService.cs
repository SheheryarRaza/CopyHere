using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CopyHere.Application.DTO.Clipboard;
using CopyHere.Application.DTO.Device;
using CopyHere.Application.Interfaces.Services;
using CopyHere.Core.Entity;
using CopyHere.Core.Exceptions;
using CopyHere.Core.Interfaces.IRepositories;

namespace CopyHere.Application.Services
{
    public class ClipboardService : IClipboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ClipboardService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<DTO_ClipboardEntry> UploadClipboardEntryAsync (Guid userId, DTO_UploadClipboardRequest request)
        {
            var device = await _unitOfWork.Devices.GetByIdAsync(request.DeviceId);
            if(device == null || device.UserId != userId)
            {
                throw new UnauthorizedAccessException("Device not found or does not belong to the user.");
            }

            var entry = new ClipboardEntry
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                DeviceId = request.DeviceId,
                ContentType = request.ContentType,
                ContentText = request.ContentText ?? string.Empty,
                ContentBytes = request.ContentBase64 != null ? Convert.FromBase64String(request.ContentBase64) : null,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ClipboardEntries.AddAsync(entry);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<DTO_ClipboardEntry>(entry);
        }
        public async Task<DTO_ClipboardEntry?> GetLatestClipboardEntryAsync(Guid userId)
        {
            var entry = await _unitOfWork.ClipboardEntries.GetLatestUserClipboardEntryAsync(userId);
            if (entry == null)
            {
                return null;
            }

            return _mapper.Map<DTO_ClipboardEntry>(entry);
        }
        public async Task<IEnumerable<DTO_ClipboardEntry>> GetClipboardHistoryAsync(Guid userId, int skip = 0, int take = 10, bool includeArchived = false)
        {
            var history = await _unitOfWork.ClipboardEntries.GetUserClipboardHistoryAsync(userId, skip, take, includeArchived);
            return _mapper.Map<IEnumerable<DTO_ClipboardEntry>>(history);
        }
        public async Task DeleteClipboardEntryAsync(Guid userId, Guid entryId)
        {
            var entry = await _unitOfWork.ClipboardEntries.GetByIdAsync(entryId);
            if (entry == null || entry.UserId != userId)
            {
                throw new NotFoundException(nameof(ClipboardEntry), entryId);
            }

            await _unitOfWork.ClipboardEntries.DeleteAsync(entryId);
            await _unitOfWork.CompleteAsync(); // Commit changes
        }
        public async Task ClearAllClipboardEntriesAsync(Guid userId)
        {
            await _unitOfWork.ClipboardEntries.ClearUserClipboardAsync(userId);
            await _unitOfWork.CompleteAsync(); // Commit changes
        }
        public async Task<DTO_Device> RegisterDeviceAsync(Guid userId, DTO_RegisterDeviceRequest request)
        {
            var device = new Device
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                DeviceName = request.DeviceName,
                DeviceType = request.DeviceType,
                LastSeen = DateTime.UtcNow
            };

            await _unitOfWork.Devices.AddAsync(device);
            await _unitOfWork.CompleteAsync(); // Commit changes

            return _mapper.Map<DTO_Device>(device);
        }
        public async Task<IEnumerable<DTO_Device>> GetUserDevicesAsync(Guid userId)
        {
            var devices = await _unitOfWork.Devices.GetUserDevicesAsync(userId);
            return _mapper.Map<IEnumerable<DTO_Device>>(devices);
        }
        public async Task DeleteDeviceAsync(Guid userId, Guid deviceId)
        {
            var device = await _unitOfWork.Devices.GetByIdAsync(deviceId);
            if (device == null || device.UserId != userId)
            {
                throw new NotFoundException(nameof(Device), deviceId);
            }

            await _unitOfWork.Devices.DeleteAsync(device);
            await _unitOfWork.CompleteAsync(); // Commit changes
        }

        public async Task<DTO_ClipboardEntry> RestoreClipboardEntryAsync(Guid userId, Guid entryId)
        {
            var originalEntry = await _unitOfWork.ClipboardEntries.GetByIdAsync(entryId);
            if (originalEntry == null || originalEntry.UserId != userId)
            {
                throw new NotFoundException(nameof(ClipboardEntry), entryId);
            }

            var restoredEntry = new ClipboardEntry
            {
                Id = Guid.NewGuid(),
                UserId = originalEntry.UserId,
                DeviceId = originalEntry.DeviceId,
                ContentType = originalEntry.ContentType,
                ContentText = originalEntry.ContentText,
                ContentBytes = originalEntry.ContentBytes,
                IsPinned = false,
                IsArchived = false,
                Tags = originalEntry.Tags,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ClipboardEntries.AddAsync(restoredEntry);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<DTO_ClipboardEntry>(restoredEntry);
        }

        public async Task<DTO_ClipboardEntry> SetPinStatusAsync(Guid userId, Guid entryId, bool isPinned)
        {
            var entry = await _unitOfWork.ClipboardEntries.GetByIdAsync(entryId);
            if (entry == null || entry.UserId != userId)
            {
                throw new NotFoundException(nameof(ClipboardEntry), entryId);
            }

            entry.IsPinned = isPinned;
            await _unitOfWork.ClipboardEntries.UpdateAsync(entry);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<DTO_ClipboardEntry>(entry);
        }

        public async Task<DTO_ClipboardEntry> SetArchiveStatusAsync(Guid userId, Guid entryId, bool isArchived)
        {
            var entry = await _unitOfWork.ClipboardEntries.GetByIdAsync(entryId);
            if (entry == null || entry.UserId != userId)
            {
                throw new NotFoundException(nameof(ClipboardEntry), entryId);
            }

            entry.IsArchived = isArchived;
            await _unitOfWork.ClipboardEntries.UpdateAsync(entry);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<DTO_ClipboardEntry>(entry);
        }

        public async Task<DTO_ClipboardEntry> UpdateTagsAsync(Guid userId, Guid entryId, List<string> tags)
        {
            var entry = await _unitOfWork.ClipboardEntries.GetByIdAsync(entryId);
            if (entry == null || entry.UserId != userId)
            {
                throw new NotFoundException(nameof(ClipboardEntry), entryId);
            }

            entry.Tags = string.Join(",", tags);
            await _unitOfWork.ClipboardEntries.UpdateAsync(entry);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<DTO_ClipboardEntry>(entry);
        }
    }
}
