namespace GlucoPilot.Api.Endpoints.Insights.Insulin;

public sealed class InsulinInsightsResponse
{
    public double TotalBasalUnits { get; init; }
    public double TotalBolusUnits { get; init; }
}