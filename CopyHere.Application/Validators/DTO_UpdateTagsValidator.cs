using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CopyHere.Application.DTO.Clipboard;
using FluentValidation;

namespace CopyHere.Application.Validators
{
    public class DTO_UpdateTagsValidator : AbstractValidator<DTO_UpdateTags>
    {
        public DTO_UpdateTagsValidator()
        {
            RuleFor(x => x.Tags).NotNull().WithMessage("Tags list cannot be null.");
        }
    }
}
