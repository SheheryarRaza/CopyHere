using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CopyHere.Core.Entity;
using CopyHere.Core.Interfaces.IRepositories;
using CopyHere.Infrastructure.Data;
using Microsoft.AspNet.Identity;

namespace CopyHere.Infrastructure.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Devices = new DeviceRepository(_context);
            ClipboardEntries = new ClipboardEntryRepository(_context);
        }

        public IDeviceRepository Devices { get; private set; }
        public IClipboardEntryRepository ClipboardEntries { get; private set; }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
