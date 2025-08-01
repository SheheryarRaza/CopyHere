using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CopyHere.Core.Enumerations;

namespace CopyHere.Core.Entity
{
    public class ClipboardEntry
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid DeviceId { get; set; }
        public ContentType ContentType { get; set; }
        public string ContentText { get; set; } = string.Empty;
        public byte[]? ContentBytes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public User? User { get; set; }
        public Device? Device { get; set; }
    }
}
