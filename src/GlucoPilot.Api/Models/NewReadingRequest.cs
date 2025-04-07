using FluentValidation;
using System;

namespace GlucoPilot.Api.Models;

public class NewReadingRequest
{
    public required DateTimeOffset Created { get; set; }
    public required double GlucoseLevel { get; set; }
}

public sealed class NewReadingValidator : AbstractValidator<NewReadingRequest>
{
    public NewReadingValidator()
    {
        RuleFor(x => x.Created).LessThanOrEqualTo(_ => DateTimeOffset.UtcNow);
        RuleFor(x => x.GlucoseLevel).GreaterThan(0);
    }
}
