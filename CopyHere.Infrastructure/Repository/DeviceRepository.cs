using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CopyHere.Core.Entity;
using CopyHere.Core.Interfaces.IRepositories;
using CopyHere.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CopyHere.Infrastructure.Repository
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly ApplicationDbContext _context;

        public DeviceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Device?> GetByIdAsync(Guid id)
        {
            return await _context.Devices.FindAsync(id);
        }

        public async Task<IEnumerable<Device>> GetUserDevicesAsync(Guid userId)
        {
            var userDevices = _context.Devices
                .Where(d => d.UserId == userId)
                .OrderBy(d => d.DeviceName);

            return await userDevices.ToListAsync();
        }

        public async Task AddAsync(Device device)
        {
            await _context.Devices.AddAsync(device);
            // SaveChanges is now handled by UnitOfWork
        }

        public async Task UpdateAsync(Device device)
        {
            _context.Devices.Update(device);
            // SaveChanges is now handled by UnitOfWork
        }

        public async Task DeleteAsync(Device device)
        {
            _context.Devices.Remove(device);
            // SaveChanges is now handled by UnitOfWork
        }
    }
}
