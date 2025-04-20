namespace GlucoPilot.Api.Endpoints.Treatments.Nutrition;

public sealed class NutritionResponseModel
{
    public int TotalCalories { get; init; }
    public int TotalCarbs { get; init; }
    public int TotalProtein { get; init; }
    public int TotalFat { get; init; }
}