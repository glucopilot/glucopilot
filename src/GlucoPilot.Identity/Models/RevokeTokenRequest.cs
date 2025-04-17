using FluentValidation;

namespace GlucoPilot.Identity.Models;

public sealed record RevokeTokenRequest
{
    public string? Token { get; init; }

    public sealed class Validator : AbstractValidator<RevokeTokenRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Token).NotNull().WithMessage("TOKEN_REQUIRED").NotEmpty().WithMessage("TOKEN_REQUIRED");
        }
    }
}