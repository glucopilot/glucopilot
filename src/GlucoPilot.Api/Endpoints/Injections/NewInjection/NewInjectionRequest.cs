using FluentValidation;
using System;

namespace GlucoPilot.Api.Endpoints.Injections.NewInjection;

public record NewInjectionRequest
{
    public required Guid InsulinId { get; init; }
    public required double Units { get; init; }

    public sealed class NewInjectionRequestValidator : AbstractValidator<NewInjectionRequest>
    {
        public NewInjectionRequestValidator()
        {
            RuleFor(x => x.InsulinId)
                .NotEmpty()
                .WithMessage("InsulinId is required.");
            RuleFor(x => x.Units)
                .GreaterThan(0)
                .WithMessage("Units must be greater than 0.");
        }
    }
}