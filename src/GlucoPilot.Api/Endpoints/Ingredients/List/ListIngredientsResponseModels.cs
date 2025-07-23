using GlucoPilot.Api.Models;
using System;
using System.Collections.Generic;

namespace GlucoPilot.Api.Endpoints.Ingredients.List;

public sealed record ListIngredientsResponse : PagedResponse
{
    public required ICollection<GetIngredientResponse> Ingredients { get; set; } = [];
}

public sealed record GetIngredientResponse
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required decimal Carbs { get; set; }
    public required decimal Protein { get; set; }
    public required decimal Fat { get; set; }
    public required decimal Calories { get; set; }
    public required UnitOfMeasurement Uom { get; set; }
    public required DateTimeOffset Created { get; set; }
    public DateTimeOffset? Updated { get; set; }
}
