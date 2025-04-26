namespace GlucoPilot.Api.Endpoints.Insights.Insulin;

public sealed class InsulinInsightsResponse
{
    public double Basal { get; init; }
    public double Bolus { get; init; }
}