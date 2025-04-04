using System;
using System.ComponentModel.DataAnnotations;
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
    /// The meals the meal ingredient is associated with.
    /// </summary>
    public virtual required Meal Meal { get; set; }

    /// <summary>
    /// The ingredients the meal ingredient is associated with.
    /// </summary>
    public virtual required Ingredient Ingredient { get; set; }

    /// <summary>
    /// The quantity of the ingredient in the meal.
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Quantity must 0 or more.")]
    public required int Quantity { get; set; }

}
