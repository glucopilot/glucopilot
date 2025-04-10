using FluentValidation;
using GlucoPilot.Api.Models;
using System.Collections.Generic;
using static GlucoPilot.Api.Models.NewMealIngredientRequest;

namespace GlucoPilot.Api.Endpoints.Meals.NewMeal;

public record NewMealRequest
{
    public required string Name { get; set; }
    public ICollection<NewMealIngredientRequest> MealIngredients { get; set; } = [];

    public sealed class NewMealValidator : AbstractValidator<NewMealRequest>
    {
        public NewMealValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleForEach(x => x.MealIngredients).SetValidator(new NewMealIngredientValidator());
        }
    }
}
