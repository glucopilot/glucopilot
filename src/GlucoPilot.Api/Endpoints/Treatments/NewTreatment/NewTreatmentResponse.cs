using System;
using System.Collections.Generic;

namespace GlucoPilot.Api.Endpoints.Treatments.NewTreatment;

public record NewTreatmentResponse
{
    public required Guid Id { get; init; }
    public required DateTimeOffset Created { get; init; }
    public DateTimeOffset? Updated { get; init; }
    public ICollection<NewTreatmentMealResponse> Meals { get; init; } = [];
    public ICollection<NewTreatmentIngredientResponse> Ingredients { get; init; } = [];
    public Guid? InjectionId { get; init; }
    public string? InsulinName { get; init; }
    public double? InsulinUnits { get; init; }
    public Guid? ReadingId { get; init; }
    public double? ReadingGlucoseLevel { get; init; }
}

public record NewTreatmentMealResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required decimal Quantity { get; init; }
}
public record NewTreatmentIngredientResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required decimal Quantity { get; init; }
}
