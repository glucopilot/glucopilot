using System;
using System.Collections.Generic;

namespace GlucoPilot.Api.Endpoints.Treatments.UpdateTreatment;

public sealed record UpdateTreatmentResponse
{
    public Guid Id { get; set; }
    public ICollection<UpdateTreatmentMealResponse> Meals { get; init; } = [];
    public ICollection<UpdateTreatmentIngredientResponse> Ingredients { get; init; } = [];
    public Guid? InjectionId { get; init; }
    public string? InsulinName { get; init; }
    public double? InsulinUnits { get; init; }
    public Guid? ReadingId { get; init; }
    public double? ReadingGlucoseLevel { get; init; }
    public DateTimeOffset? Updated { get; init; }
}

public record UpdateTreatmentMealResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Quantity { get; set; }
}

public record UpdateTreatmentIngredientResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Quantity { get; set; }
}
