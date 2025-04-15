using System;

namespace GlucoPilot.Api.Endpoints.Treatments.NewTreatment;

public record NewTreatmentResponse
{
    public required Guid Id { get; init; }
    public required DateTimeOffset Created { get; init; }
    public Guid? MealId { get; init; }
    public Guid? InjectionId { get; init; }
    public Guid? ReadingId { get; init; }
}
