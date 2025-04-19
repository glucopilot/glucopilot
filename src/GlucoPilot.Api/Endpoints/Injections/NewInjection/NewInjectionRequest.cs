using FluentValidation;
using System;

namespace GlucoPilot.Api.Endpoints.Injections.NewInjection;

public record NewInjectionRequest
{
    public required Guid InsulinId { get; init; }
    public required double Units { get; init; }
    public required DateTimeOffset Created { get; init; }

    public sealed class NewInjectionRequestValidator : AbstractValidator<NewInjectionRequest>
    {
        public NewInjectionRequestValidator()
        {
            RuleFor(x => x.InsulinId)
                .NotEmpty()
                .WithMessage(Resources.ValidationMessages.InsulinIdInvalid);
            RuleFor(x => x.Units)
                .GreaterThan(0)
                .WithMessage(Resources.ValidationMessages.UnitsGreaterThanZero);
            RuleFor(x => x.Created)
                .NotEmpty()
                .WithMessage(Resources.ValidationMessages.CreatedDateRequired);
        }
    }
}