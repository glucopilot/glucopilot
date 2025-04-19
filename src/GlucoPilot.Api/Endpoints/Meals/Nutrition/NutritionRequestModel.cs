using System;
using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace GlucoPilot.Api.Endpoints.Meals.Nutrition;

public sealed class NutritionRequestModel
{
    [Required]
    public DateTimeOffset From { get; set; }

    public DateTimeOffset To { get; set; } = DateTimeOffset.UtcNow;

    public sealed class Validator : AbstractValidator<NutritionRequestModel>
    {
        public Validator()
        {
            RuleFor(x => x.From).LessThan(x => x.To).WithMessage(Resources.ValidationMessages.ToBeforeFrom);
        }
    }
}