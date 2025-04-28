using FluentValidation;
using GlucoPilot.Api.Models;
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
                .WithMessage(Resources.ValidationMessages.InsulinIdInvalid);
            RuleFor(x => x.Units)
                .GreaterThan(0)
                .WithMessage(Resources.ValidationMessages.UnitsGreaterThanZero);
            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage(Resources.ValidationMessages.InsulinTypeInvalid);
        }
    }
}