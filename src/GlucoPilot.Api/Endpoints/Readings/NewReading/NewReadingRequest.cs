using FluentValidation;
using System;

namespace GlucoPilot.Api.Endpoints.Readings.NewReading;

public class NewReadingRequest
{
    public required DateTimeOffset Created { get; set; }
    public required double GlucoseLevel { get; set; }
}

public sealed class NewReadingValidator : AbstractValidator<NewReadingRequest>
{
    public NewReadingValidator()
    {
        RuleFor(x => x.Created).LessThanOrEqualTo(_ => DateTimeOffset.UtcNow).WithMessage(Resources.ValidationMessages.CannotBeFutureDate);
        RuleFor(x => x.GlucoseLevel).GreaterThan(0).WithMessage(Resources.ValidationMessages.LevelGreaterThanZero);
    }
}
