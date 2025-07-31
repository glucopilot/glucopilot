using GlucoPilot.Api.Models;
using System;
using System.Collections.Generic;

namespace GlucoPilot.Api.Endpoints.Insights.TimeInRange;

public record TimeInRangeResponse
{
    public required DateTimeOffset From { get; init; }
    public required DateTimeOffset To { get; init; }
    public required ICollection<GlucoseRange> Ranges { get; init; } = [];
}
