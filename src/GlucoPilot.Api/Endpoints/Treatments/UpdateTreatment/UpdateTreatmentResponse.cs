using System;

namespace GlucoPilot.Api.Endpoints.Treatments.UpdateTreatment;

public sealed record UpdateTreatmentResponse
{
    public Guid Id { get; set; }
    public Guid? MealId { get; init; }
    public string? MealName { get; init; }
    public Guid? InjectionId { get; init; }
    public string? InsulinName { get; init; }
    public double? InsulinUnits { get; init; }
    public Guid? ReadingId { get; init; }
    public double? ReadingGlucoseLevel { get; init; }
    public DateTimeOffset? Updated { get; init; }
}
