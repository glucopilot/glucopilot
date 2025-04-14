using FluentValidation;
using GlucoPilot.Api.Endpoints.Injections.NewInjection;
using GlucoPilot.Data.Enums;
using System;

namespace GlucoPilot.Api.Endpoints.Injections.UpdateInjection;

public record UpdateInjectionRequest
{
    public required Guid InsulinId { get; init; }
    public required double Units { get; init; }
    public required InsulinType Type { get; init; }

    public sealed class UpdateInjectionRequestValidator : AbstractValidator<UpdateInjectionRequest>
    {
        public UpdateInjectionRequestValidator()
        {
            RuleFor(x => x.InsulinId)
                .NotEmpty()
                .WithMessage("InsulinId is required.");
            RuleFor(x => x.Units)
                .GreaterThan(0)
                .WithMessage("Units must be greater than 0.");
            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Type must be a valid InsulinType.");
        }
    }
}