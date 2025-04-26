using System;

namespace GlucoPilot.Api.Endpoints.Insights.AverageGlucose;

public sealed class AverageGlucoseRequest
{
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
}