using FluentValidation;
using GlucoPilot.Api.Endpoints.Ingredients.List;
using System;
using System.Collections.Generic;

namespace GlucoPilot.Api.Endpoints.Ingredients.GetIngredients;

public sealed record GetIngredientsRequest
{
    public required ICollection<Guid> Ids { get; set; } = [];
    public sealed class ListIngredientsValidator : AbstractValidator<ListIngredientsResponse>
    {
        public ListIngredientsValidator()
        {
            RuleFor(x => x.Ingredients).NotEmpty().WithMessage(Resources.ValidationMessages.IngredientsNotEmpty);
        }
    }
}
