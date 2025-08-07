using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopyHere.Application.DTO.Device
{
    public class DTO_RegisterDeviceRequest
    {
        public string DeviceName { get; set; } = string.Empty;

        public string DeviceType { get; set; } = string.Empty; // e.g., "Mobile", "PC"
    }
}
