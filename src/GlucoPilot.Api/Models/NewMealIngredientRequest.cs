using FluentValidation;
using System;

namespace GlucoPilot.Api.Models;

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
