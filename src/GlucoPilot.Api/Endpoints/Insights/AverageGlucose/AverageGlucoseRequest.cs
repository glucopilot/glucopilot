using System;
using FluentValidation;

namespace GlucoPilot.Api.Endpoints.Insights.AverageGlucose;

public sealed class AverageGlucoseRequest
{
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }

    public sealed class Validator : AbstractValidator<AverageGlucoseRequest>
    {
        public Validator()
        {
            RuleFor(x => x.From).LessThan(x => x.To).WithMessage(Resources.ValidationMessages.ToBeforeFrom);
        }
    }
}