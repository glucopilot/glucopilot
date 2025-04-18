using GlucoPilot.Api.Models;
using GlucoPilot.Data.Enums;
using System;
using System.Collections.Generic;

namespace GlucoPilot.Api.Endpoints.Meals.List
{
    public sealed record ListMealsResponse : PagedResponse
    {
        public required ICollection<GetMealResponse> Meals { get; set; }
    }

    public record GetMealResponse
    {
        public required Guid Id { get; set; }
        public required string Name { get; set; }
        public required DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }
        public required int TotalCalories { get; set; }
        public required int TotalCarbs { get; set; }
        public required int TotalProtein { get; set; }
        public required int TotalFat { get; set; }
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
        public required int Carbs { get; set; }
        public required int Protein { get; set; }
        public required int Fat { get; set; }
        public required int Calories { get; set; }
        public required UnitOfMeasurement Uom { get; set; }
        public DateTimeOffset? Created { get; set; }
        public DateTimeOffset? Updated { get; set; }
    }
}
