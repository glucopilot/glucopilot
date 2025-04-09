using FluentValidation;

namespace GlucoPilot.Identity.Models;

public sealed record LoginRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }

    internal sealed class Validator : AbstractValidator<LoginRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage("EMAIL_REQUIRED").EmailAddress().WithMessage("EMAIL_INVALID");
            RuleFor(x => x.Password).NotEmpty().WithMessage("PASSWORD_REQUIRED");
        }
    }
}