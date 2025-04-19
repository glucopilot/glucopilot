using FluentValidation;
using System;
using System.Collections.Generic;

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
        }
    }
}

public sealed record NewMealIngredientRequest
{
    public required Guid IngredientId { get; set; }
    public required int Quantity { get; set; }

    public sealed class NewMealIngredientValidator : AbstractValidator<NewMealIngredientRequest>
    {
        public NewMealIngredientValidator()
        {
            RuleFor(x => x.IngredientId).NotEmpty().WithMessage(Resources.ValidationMessages.IngredientIdInvalid);
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage(Resources.ValidationMessages.QuantityGreaterThanZero);
        }
    }
}
