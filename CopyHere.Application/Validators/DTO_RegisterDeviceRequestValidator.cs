using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CopyHere.Application.DTO.Device;
using FluentValidation;

namespace CopyHere.Application.Validators
{
    public class DTO_RegisterDeviceRequestValidator : AbstractValidator<DTO_RegisterDeviceRequest>
    {
        public DTO_RegisterDeviceRequestValidator()
        {
            RuleFor(x => x.DeviceName).NotEmpty().WithMessage("Device name is required.");
            RuleFor(x => x.DeviceType).NotEmpty().WithMessage("Device type is required.");
        }
    }
}
