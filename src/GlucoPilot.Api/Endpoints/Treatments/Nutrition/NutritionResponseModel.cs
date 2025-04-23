namespace GlucoPilot.Api.Endpoints.Treatments.Nutrition;

public sealed class NutritionResponseModel
{
    public decimal TotalCalories { get; init; }
    public decimal TotalCarbs { get; init; }
    public decimal TotalProtein { get; init; }
    public decimal TotalFat { get; init; }
}