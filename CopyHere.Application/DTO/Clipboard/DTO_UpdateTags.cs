using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopyHere.Application.DTO.Clipboard
{
    public class DTO_UpdateTags
    {
        [Required]
        public List<string> Tags { get; set; } = new List<string>();
    }
}
