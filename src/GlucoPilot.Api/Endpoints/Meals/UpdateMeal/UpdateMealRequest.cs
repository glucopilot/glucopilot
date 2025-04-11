using FluentValidation;
using System.Collections.Generic;
using GlucoPilot.Api.Models;
using System;

namespace GlucoPilot.Api.Endpoints.Meals.UpdateMeal;

public sealed record UpdateMealRequest
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public ICollection<NewMealIngredientRequest> MealIngredients { get; set; } = [];

    public sealed class UpdateMealRequestValidator : AbstractValidator<UpdateMealRequest>
    {
        public UpdateMealRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleForEach(x => x.MealIngredients).SetValidator(new NewMealIngredientRequest.NewMealIngredientValidator());
        }
    }
}
