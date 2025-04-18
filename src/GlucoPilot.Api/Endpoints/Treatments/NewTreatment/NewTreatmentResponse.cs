using System;

namespace GlucoPilot.Api.Endpoints.Treatments.NewTreatment;

public record NewTreatmentResponse
{
    public required Guid Id { get; init; }
    public required DateTimeOffset Created { get; init; }
    public DateTimeOffset? Updated { get; init; }
    public Guid? MealId { get; init; }
    public string? MealName { get; init; }
    public Guid? InjectionId { get; init; }
    public string? InsulinName { get; init; }
    public double? InsulinUnits { get; init; }
    public Guid? ReadingId { get; init; }
    public double? ReadingGlucoseLevel { get; init; }
}
