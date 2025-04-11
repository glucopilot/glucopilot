using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace GlucoPilot.Identity.Models;

public sealed class VerifyEmailRequest
{
    [Required] public required string Token { get; init; }

    public sealed class Validator : AbstractValidator<VerifyEmailRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Token).NotEmpty().WithMessage("TOKEN_REQUIRED");
        }
    }
}