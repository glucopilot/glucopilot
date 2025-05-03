namespace GlucoPilot.Api.Endpoints.Insights.AverageNutrition;

public sealed class AverageNutritionResponse
{
    public decimal Calories { get; init; }
    public decimal Carbs { get; init; }
    public decimal Protein { get; init; }
    public decimal Fat { get; init; }
}