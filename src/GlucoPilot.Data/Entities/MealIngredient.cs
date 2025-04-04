using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlucoPilot.Data.Entities;

[Table("meal_ingredient")]
public class MealIngredient
{
    /// <summary>
    /// The unique identifier for the meal ingredient.
    /// </summary>
    public required Guid MealIngredientId { get; set; }

    /// <summary>
    /// The list of meals the meal ingredient is associated with.
    /// </summary>
    public List<Meal> Meals { get; set; } = [];

    /// <summary>
    /// The list of ingredients the meal ingredient is associated with.
    /// </summary>
    public List<Ingredient> Ingredients { get; set; } = [];

}
