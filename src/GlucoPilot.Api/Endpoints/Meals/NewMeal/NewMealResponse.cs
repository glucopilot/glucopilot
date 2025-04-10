using System;

namespace GlucoPilot.Api.Endpoints.Meals.NewMeal;

public record NewMealResponse
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
}
