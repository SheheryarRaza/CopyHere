using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CopyHere.Application.DTO.Auth;
using FluentValidation;

namespace CopyHere.Application.Validators
{
    public class DTO_LoginRequestValidator : AbstractValidator<DTO_LoginRequest>
    {
        public DTO_LoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");
        }
    }
}
