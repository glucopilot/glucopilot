using FluentValidation;

namespace GlucoPilot.Identity.Models;

public sealed record LoginRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }

    public sealed class Validator : AbstractValidator<LoginRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage(Resources.ValidationMessages.EmailRequired).EmailAddress().WithMessage(Resources.ValidationMessages.EmailInvalid);
            RuleFor(x => x.Password).NotEmpty().WithMessage(Resources.ValidationMessages.PasswordRequired);
        }
    }
}