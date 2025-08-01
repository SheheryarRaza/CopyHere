using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopyHere.Application.Common.Settings
{
    public class ApplicationSettings
    {
        public string JwtSecret { get; set; } = string.Empty;
        public int JwtTokenExpiryMinutes { get; set; }
    }
}
