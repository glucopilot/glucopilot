using FluentValidation;
using System;

namespace GlucoPilot.Api.Endpoints.Insights.TotalNutrition
{
    public sealed class TotalNutritionResponse
    {
        public decimal TotalCalories { get; init; }
        public decimal TotalCarbs { get; init; }
        public decimal TotalFat { get; init; }
        public decimal TotalProtein { get; init; }
    }
}
