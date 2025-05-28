using System;

namespace GlucoPilot.Api.Endpoints.Readings.NewReading;

public sealed record NewReadingResponse
{
    public Guid Id { get; set; }
    public DateTimeOffset Created { get; set; }
    public double GlucoseLevel { get; set; }
}
