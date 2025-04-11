using GlucoPilot.Data.Enums;
using System;

namespace GlucoPilot.Api.Endpoints.Ingredients.NewIngredient;

public sealed record NewIngredientResponse
{
    public required Guid Id { get; set; }
    public required DateTimeOffset Created { get; set; }
    public required string Name { get; set; }
    public int Carbs { get; set; }
    public int Protein { get; set; }
    public int Fat { get; set; }
    public int Calories { get; set; }
    public required UnitOfMeasurement Uom { get; set; }
}
