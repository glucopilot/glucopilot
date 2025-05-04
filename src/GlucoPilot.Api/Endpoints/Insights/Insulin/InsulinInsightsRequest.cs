using System;
using FluentValidation;

namespace GlucoPilot.Api.Endpoints.Insights.Insulin;

public sealed class InsulinInsightsRequest
{
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }

    public sealed class Validator : AbstractValidator<InsulinInsightsRequest>
    {
        public Validator()
        {
            RuleFor(x => x.From).LessThan(x => x.To).WithMessage(Resources.ValidationMessages.ToBeforeFrom);
        }
    }
}