using GlucoPilot.Data.Enums;
using System;
using System.Collections.Generic;

namespace GlucoPilot.Api.Endpoints.Meals.GetMeal;

public record GetMealResponse
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required DateTimeOffset Created { get; set; }
    public DateTimeOffset? Updated { get; set; }
    public required decimal TotalCalories { get; set; }
    public required decimal TotalCarbs { get; set; }
    public required decimal TotalProtein { get; set; }
    public required decimal TotalFat { get; set; }
    public List<MealIngredientResponse>? MealIngredients { get; set; } = new();
}

public record MealIngredientResponse
{
    public required Guid Id { get; set; }

    public required IngredientResponse? Ingredient { get; set; }

    public required int Quantity { get; set; }
}

public record IngredientResponse
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required decimal Carbs { get; set; }
    public required decimal Protein { get; set; }
    public required decimal Fat { get; set; }
    public required decimal Calories { get; set; }
    public required UnitOfMeasurement Uom { get; set; }
    public DateTimeOffset? Created { get; set; }
    public DateTimeOffset? Updated { get; set; }
}
