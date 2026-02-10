using FluentValidation;
using System;

namespace GlucoPilot.Api.Endpoints.Insights.TotalNutrition
{
    public class TotalNutritionRequest
    {
        public DateTimeOffset? From { get; set; }
        public DateTimeOffset? To { get; set; }

        public sealed class Validator : AbstractValidator<TotalNutritionRequest>
        {
            public Validator()
            {
                RuleFor(x => x.From).LessThan(x => x.To).WithMessage(Resources.ValidationMessages.ToBeforeFrom);
            }
        }
    }
}
