using System;
using FluentValidation;

namespace GlucoPilot.Api.Endpoints.Insights.InsulinToCarbRatio;

public sealed class InsulinToCarbRatioRequest
{
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }

    public sealed class Validator : AbstractValidator<InsulinToCarbRatioRequest>
    {
        public Validator()
        {
            RuleFor(x => x.From).LessThan(x => x.To).WithMessage(Resources.ValidationMessages.ToBeforeFrom);
        }
    }
}