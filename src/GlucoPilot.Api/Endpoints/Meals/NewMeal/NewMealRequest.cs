using FluentValidation;
using GlucoPilot.Data.Entities;
using System.Collections.Generic;

namespace GlucoPilot.Api.Endpoints.Meals.NewMeal;

public record NewMealRequest
{
    public required string Name { get; set; }
    public ICollection<MealIngredient> MealIngredients { get; set; } = [];

    public sealed class NewMealValidator : AbstractValidator<NewMealRequest>
    {
        public NewMealValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }
}
