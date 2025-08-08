using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CopyHere.Application.DTO.Clipboard;
using CopyHere.Core.Enumerations;
using FluentValidation;

namespace CopyHere.Application.Validators
{
    public class DTO_UploadClipboardRequestValidator : AbstractValidator<DTO_UploadClipboardRequest>
    {
        public DTO_UploadClipboardRequestValidator()
        {
            RuleFor(x => x.DeviceId).NotEmpty().WithMessage("Device ID is required.");
            RuleFor(x => x.ContentType).IsInEnum().WithMessage("Invalid content type.");

            // Custom logic to ensure content is provided based on type
            RuleFor(x => x.ContentText)
                .NotEmpty()
                .When(x => x.ContentType == ContentType.Text || x.ContentType == ContentType.Html)
                .WithMessage("Content text is required for this content type.");

            RuleFor(x => x.ContentBase64)
                .NotEmpty()
                .When(x => x.ContentType == ContentType.Image || x.ContentType == ContentType.File)
                .WithMessage("Base64 content is required for this content type.");
        }
    }
}
