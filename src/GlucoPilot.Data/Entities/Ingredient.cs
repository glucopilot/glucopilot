using GlucoPilot.Data.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlucoPilot.Data.Entities;

/// <summary>
/// Ingredient represents a single food item that can be used in a meal.
/// </summary>
[Table("ingredients")]
public class Ingredient
{
    /// <summary>
    /// The unique identifier for the ingredient.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The unique identifier for the user who created the ingredient.
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// The user who created the ingredient.
    /// </summary>
    public virtual User? User { get; set; }

    /// <summary>
    /// The date and time the ingredient was created.
    /// </summary>
    public required DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// The name of the ingredient.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The number of carbs in an ingredient.
    /// </summary>
    public int Carbs { get; set; } = 0;

    /// <summary>
    /// The amount of protein in an ingredient.
    /// </summary>
    public int Protein { get; set; } = 0;

    /// <summary>
    /// The amount of fat in an ingredient.
    /// </summary>
    public int Fat { get; set; } = 0;

    /// <summary>
    /// The amount of calories in an ingredient.
    /// </summary>
    public int Calories { get; set; } = 0;

    /// <summary>
    /// The unit of measurement of the ingredient that the nutritional values are based on.
    /// </summary>
    public required UnitOfMeasurement Uom { get; set; }

    /// <summary>
    /// The list of ingredients associated with this ingredient.
    /// </summary>
    public virtual ICollection<MealIngredient> Meals { get; set; } = [];
}
