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

    public sealed class NewInsulinRequestValidator: AbstractValidator<NewInsulinRequest>
    {
        public NewInsulinRequestValidator()
        {
            RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(100)
            .WithMessage("Name must be less than 100 characters.");
            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Type must be either Bolus or Basal.");
            RuleFor(x => x.Duration)
                .GreaterThan(0)
                .WithMessage("Duration must be greater than 0.")
                .When(x => x.Duration.HasValue);
            RuleFor(x => x.Scale)
                .GreaterThan(0)
                .WithMessage("Scale must be greater than 0.")
                .When(x => x.Scale.HasValue);
            RuleFor(x => x.PeakTime)
                .GreaterThan(0)
                .WithMessage("PeakTime must be greater than 0.")
                .When(x => x.PeakTime.HasValue);
        }
    }
}
