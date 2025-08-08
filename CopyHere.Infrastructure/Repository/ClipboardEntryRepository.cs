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
    public class ClipboardEntryRepository : IClipboardEntryRepository
    {
        private readonly ApplicationDbContext _context;

        public ClipboardEntryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ClipboardEntry?> GetByIdAsync(Guid id)
        {
            return await _context.ClipboardEntries.FindAsync(id);
        }

        public async Task<IEnumerable<ClipboardEntry>> GetUserClipboardHistoryAsync(Guid userId, int skip, int take, bool includeArchived = false)
        {
            var query = _context.ClipboardEntries.Where(ce => ce.UserId == userId);

            if (!includeArchived)
            {
                query = query.Where(ce => !ce.IsArchived);
            }

            // Pinned items first, then by date
            return await query
                .OrderByDescending(ce => ce.IsPinned)
                .ThenByDescending(ce => ce.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<ClipboardEntry?> GetLatestUserClipboardEntryAsync(Guid userId)
        {
            return await _context.ClipboardEntries
                .Where(ce => ce.UserId == userId)
                .OrderByDescending(ce => ce.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task AddAsync(ClipboardEntry entry)
        {
            await _context.ClipboardEntries.AddAsync(entry);
            // SaveChanges is now handled by UnitOfWork
        }

        public async Task UpdateAsync(ClipboardEntry entry)
        {
            _context.ClipboardEntries.Update(entry);
            // SaveChanges is now handled by UnitOfWork
        }

        public async Task DeleteAsync(Guid id)
        {
            var entry = await _context.ClipboardEntries.FindAsync(id);
            if (entry != null)
            {
                _context.ClipboardEntries.Remove(entry);
                // SaveChanges is now handled by UnitOfWork
            }
        }

        public async Task ClearUserClipboardAsync(Guid userId)
        {
            // Efficiently delete all entries for a user
            await _context.ClipboardEntries
                .Where(ce => ce.UserId == userId)
                .ExecuteDeleteAsync(); // Requires EF Core 7+
            // For older EF Core versions:
            // var entriesToDelete = await _context.ClipboardEntries.Where(ce => ce.UserId == userId).ToListAsync();
            // _context.ClipboardEntries.RemoveRange(entriesToDelete);
            // SaveChanges is now handled by UnitOfWork
        }
    }
}
