using System;

namespace GlucoPilot.Api.Models
{
    public class MealIngredientResponse
    {
        public required Guid Id { get; set; }

        public required IngredientResponse? Ingredient { get; set; }

        public required int Quantity { get; set; }
    }
}
