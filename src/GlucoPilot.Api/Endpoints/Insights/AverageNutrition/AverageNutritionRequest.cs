using System;
using FluentValidation;

namespace GlucoPilot.Api.Endpoints.Insights.AverageNutrition;

public sealed class AverageNutritionRequest
{
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
    public TimeSpan? Range { get; set; }

    public sealed class Validator : AbstractValidator<AverageNutritionRequest>
    {
        public Validator()
        {
            RuleFor(x => x.From).LessThan(x => x.To).WithMessage(Resources.ValidationMessages.ToBeforeFrom);
            RuleFor(x => x.Range).GreaterThan(TimeSpan.Zero).WithMessage(Resources.ValidationMessages.RangeGreaterThanZero);
        }
    }
}