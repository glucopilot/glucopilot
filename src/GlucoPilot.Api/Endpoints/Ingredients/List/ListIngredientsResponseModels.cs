using GlucoPilot.Api.Models;
using GlucoPilot.Data.Enums;
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
    public required int Carbs { get; set; }
    public required int Protein { get; set; }
    public required int Fat { get; set; }
    public required int Calories { get; set; }
    public required UnitOfMeasurement Uom { get; set; }
    public required DateTimeOffset Created { get; set; }
    public DateTimeOffset? Updated { get; set; }
}
