using System;

namespace GlucoPilot.Api.Endpoints.Insights.AverageNutrition;

public sealed class AverageNutritionRequest
{
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
    public TimeSpan Range { get; set; }
}