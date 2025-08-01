using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopyHere.Core.Interfaces.IRepositories
{
    public interface IUnitOfWork : IDisposable
    {
        IDeviceRepository Devices { get; }
        IClipboardEntryRepository ClipboardEntries { get; }

        Task<int> CompleteAsync();
    }
}
