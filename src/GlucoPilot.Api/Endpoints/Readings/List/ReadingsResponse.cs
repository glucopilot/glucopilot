using System;
using GlucoPilot.Api.Models;

namespace GlucoPilot.Api.Endpoints.Readings.List;

public sealed record ReadingsResponse
{
    public Guid UserId { get; init; }
    public Guid Id { get; init; }
    public DateTimeOffset Created { get; init; }
    public double GlucoseLevel { get; init; }
    public ReadingDirection Direction { get; init; }
}
