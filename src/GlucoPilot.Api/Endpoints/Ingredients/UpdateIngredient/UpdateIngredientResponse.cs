using GlucoPilot.Data.Enums;
using System;

namespace GlucoPilot.Api.Endpoints.Ingredients.UpdateIngredient;

public sealed record UpdateIngredientResponse
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required decimal Carbs { get; set; }
    public required decimal Protein { get; set; }
    public required decimal Fat { get; set; }
    public required decimal Calories { get; set; }
    public required UnitOfMeasurement Uom { get; set; }
    public DateTimeOffset? Updated { get; set; }
}
