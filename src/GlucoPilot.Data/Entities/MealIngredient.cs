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
    public required Guid Id { get; set; }

    /// <summary>
    /// the unique identifier for the meal.
    /// </summary>
    public required Guid MealId { get; set; }
    
    /// <summary>
    /// The meals the meal ingredient is associated with.
    /// </summary>
    public virtual Meal? Meal { get; set; }

    /// <summary>
    /// The unique identifier for the ingredient.
    /// </summary>
    public required Guid IngredientId { get; set; }

    /// <summary>
    /// The ingredients the meal ingredient is associated with.
    /// </summary>
    public virtual Ingredient? Ingredient { get; set; }

    /// <summary>
    /// The quantity of the ingredient in the meal.
    /// </summary>
    [Range(0, int.MaxValue)]
    public required int Quantity { get; set; }

}
