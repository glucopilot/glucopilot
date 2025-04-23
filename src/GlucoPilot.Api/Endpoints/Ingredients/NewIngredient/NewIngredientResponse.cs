using GlucoPilot.Data.Enums;
using System;

namespace GlucoPilot.Api.Endpoints.Ingredients.NewIngredient;

public sealed record NewIngredientResponse
{
    public required Guid Id { get; set; }
    public required DateTimeOffset Created { get; set; }
    public required string Name { get; set; }
    public decimal Carbs { get; set; }
    public decimal Protein { get; set; }
    public decimal Fat { get; set; }
    public decimal Calories { get; set; }
    public required UnitOfMeasurement Uom { get; set; }
    public DateTimeOffset? Updated { get; set; }
}
