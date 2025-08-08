using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CopyHere.Core.Entity;

namespace CopyHere.Core.Interfaces.IRepositories
{
    public interface IClipboardEntryRepository
    {
        Task<ClipboardEntry?> GetByIdAsync(Guid id);
        Task<IEnumerable<ClipboardEntry>> GetUserClipboardHistoryAsync(Guid userId, int skip, int take, bool includeArchived = false);
        Task<ClipboardEntry?> GetLatestUserClipboardEntryAsync(Guid userId);
        Task AddAsync(ClipboardEntry entry);
        Task UpdateAsync(ClipboardEntry entry);
        Task DeleteAsync(Guid id);
        Task ClearUserClipboardAsync(Guid userId);
    }
}
