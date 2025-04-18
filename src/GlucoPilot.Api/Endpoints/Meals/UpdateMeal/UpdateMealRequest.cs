using FluentValidation;
using System.Collections.Generic;
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

public sealed record NewMealIngredientRequest
{
    public required Guid Id { get; set; }
    public required Guid IngredientId { get; set; }
    public required int Quantity { get; set; }

    public sealed class NewMealIngredientValidator : AbstractValidator<NewMealIngredientRequest>
    {
        public NewMealIngredientValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.IngredientId).NotEmpty();
            RuleFor(x => x.Quantity).GreaterThan(0);
        }
    }
}
