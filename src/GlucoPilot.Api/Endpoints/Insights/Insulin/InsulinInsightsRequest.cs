using System;

namespace GlucoPilot.Api.Endpoints.Insights.Insulin;

public sealed class InsulinInsightsRequest
{
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
}