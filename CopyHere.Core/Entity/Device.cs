using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopyHere.Core.Entity
{
    public class Device
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty; // e.g., "Mobile", "PC", "Web"
        public DateTime LastSeen { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User? User { get; set; }
        public ICollection<ClipboardEntry> ClipboardEntries { get; set; } = new List<ClipboardEntry>();
    }
}
