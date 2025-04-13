using FluentValidation;
using GlucoPilot.Data.Enums;

namespace GlucoPilot.Api.Endpoints.Insulins.UpdateInsulin;

public record UpdateInsulinRequest
{
    public required string Name { get; set; }
    public required InsulinType Type { get; set; }
    public double? Duration { get; set; }
    public double? Scale { get; set; }
    public double? PeakTime { get; set; }

    public class UpdateInsulinRequestValidator : AbstractValidator<UpdateInsulinRequest>
    {
        public UpdateInsulinRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Name is required");
            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Type must be a valid InsulinType enum value");
            RuleFor(x => x.Duration)
                .GreaterThan(0)
                .When(x => x.Duration.HasValue)
                .WithMessage("Duration must be greater than 0");
            RuleFor(x => x.Scale)
                .GreaterThan(0)
                .When(x => x.Scale.HasValue)
                .WithMessage("Scale must be greater than 0");
            RuleFor(x => x.PeakTime)
                .GreaterThan(0)
                .When(x => x.PeakTime.HasValue)
                .WithMessage("PeakTime must be greater than 0");
        }
    }
}
