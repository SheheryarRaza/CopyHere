using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public ClipboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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

            return new DTO_ClipboardEntry
            {
                Id = entry.Id,
                UserId = entry.UserId,
                DeviceId = entry.DeviceId,
                ContentType = entry.ContentType,
                ContentText = entry.ContentText,
                ContentBase64 = entry.ContentBytes != null ? Convert.ToBase64String(entry.ContentBytes) : null,
                CreatedAt = entry.CreatedAt
            };
        }
        public async Task<DTO_ClipboardEntry?> GetLatestClipboardEntryAsync(Guid userId)
        {
            var entry = await _unitOfWork.ClipboardEntries.GetLatestUserClipboardEntryAsync(userId);
            if (entry == null)
            {
                return null;
            }

            return new DTO_ClipboardEntry
            {
                Id = entry.Id,
                UserId = entry.UserId,
                DeviceId = entry.DeviceId,
                ContentType = entry.ContentType,
                ContentText = entry.ContentText,
                ContentBase64 = entry.ContentBytes != null ? Convert.ToBase64String(entry.ContentBytes) : null,
                CreatedAt = entry.CreatedAt
            };
        }
        public async Task<IEnumerable<DTO_ClipboardEntry>> GetClipboardHistoryAsync(Guid userId, int skip = 0, int take = 10)
        {
            var history = await _unitOfWork.ClipboardEntries.GetUserClipboardHistoryAsync(userId, skip, take);
            return history.Select(entry => new DTO_ClipboardEntry
            {
                Id = entry.Id,
                UserId = entry.UserId,
                DeviceId = entry.DeviceId,
                ContentType = entry.ContentType,
                ContentText = entry.ContentText,
                ContentBase64 = entry.ContentBytes != null ? Convert.ToBase64String(entry.ContentBytes) : null,
                CreatedAt = entry.CreatedAt
            });
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

            return new DTO_Device
            {
                Id = device.Id,
                UserId = device.UserId,
                DeviceName = device.DeviceName,
                DeviceType = device.DeviceType,
                LastSeen = device.LastSeen
            };
        }
        public async Task<IEnumerable<DTO_Device>> GetUserDevicesAsync(Guid userId)
        {
            var devices = await _unitOfWork.Devices.GetUserDevicesAsync(userId);
            return devices.Select(d => new DTO_Device
            {
                Id = d.Id,
                UserId = d.UserId,
                DeviceName = d.DeviceName,
                DeviceType = d.DeviceType,
                LastSeen = d.LastSeen
            });
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

    }
}
