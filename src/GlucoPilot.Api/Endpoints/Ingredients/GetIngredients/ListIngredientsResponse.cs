using GlucoPilot.Api.Models;
using System.Collections;
using System.Collections.Generic;

namespace GlucoPilot.Api.Endpoints.Ingredients.GetIngredients;

public sealed record ListIngredientsResponse : PagedResponse
{
    public required ICollection<IngredientResponse> Ingredients { get; set; } = [];
}
