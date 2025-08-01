using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CopyHere.Core.Enumerations;

namespace CopyHere.Application.DTO.Clipboard
{
    public class DTO_UploadClipboardRequest
    {
        [Required]
        public Guid DeviceId { get; set; }
        [Required]
        public ContentType ContentType { get; set; }
        public string? ContentText { get; set; }
        public string? ContentBase64 { get; set; }
    }
}
