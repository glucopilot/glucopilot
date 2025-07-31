using FluentValidation;
using System;

namespace GlucoPilot.Api.Endpoints.Insights.TimeInRange;

public record TimeInRangeRequest
{
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }

    public sealed class Validator : AbstractValidator<TimeInRangeRequest>
    {
        public Validator()
        {
            RuleFor(x => x.From).LessThan(x => x.To).WithMessage(Resources.ValidationMessages.ToBeforeFrom);
        }
    }
}
