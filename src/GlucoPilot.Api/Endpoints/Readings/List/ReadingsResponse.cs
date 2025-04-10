using GlucoPilot.Data.Enums;
using System;
using System.Collections;
using System.Collections.Generic;

namespace GlucoPilot.Api.Endpoints.Readings.List;

public sealed record ReadingsResponse
{
    public Guid UserId { get; init; }
    public Guid Id { get; init; }
    public DateTimeOffset Created { get; init; }
    public double GlucoseLevel { get; init; }
    public ReadingDirection Direction { get; init; }
}
