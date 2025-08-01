using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CopyHere.Core.Enumerations;

namespace CopyHere.Application.DTO.Clipboard
{
    public class DTO_ClipboardEntry
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid DeviceId { get; set; }
        public ContentType ContentType { get; set; }
        public string ContentText { get; set; } = string.Empty;
        public string? ContentBase64 { get; set; } // Base64 encoded string for binary content
        public DateTime CreatedAt { get; set; }
    }
}
