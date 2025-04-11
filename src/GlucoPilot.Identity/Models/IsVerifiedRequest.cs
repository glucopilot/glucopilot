using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace GlucoPilot.Identity.Models;

public sealed class IsVerifiedRequest
{
    [Required] public required string Email { get; init; }

    public sealed class Validator : AbstractValidator<IsVerifiedRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage("EMAIL_REQUIRED").EmailAddress().WithMessage("EMAIL_INVALID");
        }
    }
}