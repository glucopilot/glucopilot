using System;
using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace GlucoPilot.Api.Endpoints.Meals.Nutrition;

public sealed class NutritionRequestModel
{
    [Required]
    public DateTimeOffset From { get; set; }

    [Required]
    public DateTimeOffset? To { get; set; }
    
    public sealed class Validator : AbstractValidator<NutritionRequestModel>
    {
        public Validator()
        {
            RuleFor(x => x.From).LessThan(x => x.To).WithMessage("TO_BEFORE_FROM");
        }
    }
}