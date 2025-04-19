using FluentValidation;
using GlucoPilot.Data.Enums;

namespace GlucoPilot.Api.Endpoints.Insulins.NewInsulin;

public sealed record NewInsulinRequest
{
    public required string Name { get; init; }
    public required InsulinType Type { get; init; }
    public double? Duration { get; init; }
    public double? Scale { get; init; }
    public double? PeakTime { get; init; }

    public sealed class NewInsulinRequestValidator : AbstractValidator<NewInsulinRequest>
    {
        public NewInsulinRequestValidator()
        {
            RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(Resources.ValidationMessages.NameRequired);
            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage(Resources.ValidationMessages.InsulinTypeInvalid);
            RuleFor(x => x.Duration)
                .GreaterThan(0)
                .WithMessage(Resources.ValidationMessages.DurationGreaterThanZero)
                .When(x => x.Duration.HasValue);
            RuleFor(x => x.Scale)
                .GreaterThan(0)
                .WithMessage(Resources.ValidationMessages.ScaleGreaterThanZero)
                .When(x => x.Scale.HasValue);
            RuleFor(x => x.PeakTime)
                .GreaterThan(0)
                .WithMessage(Resources.ValidationMessages.PeaktimeGreaterThanZero)
                .When(x => x.PeakTime.HasValue);
        }
    }
}
