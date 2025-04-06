using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlucoPilot.Data.Entities;

/// <summary>
/// A meal is a collection of ingredients that are consumed together.
/// </summary>
[Table("meals")]
public class Meal
{
    /// <summary>
    /// The unique identifier for the meal.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The id of the user who created the meal.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The user who created the meal.
    /// </summary>
    public virtual User? User { get; set; }

    /// <summary>
    /// The name of the meal.
    /// </summary>
    public required string Name { get; set; } = string.Empty;

    /// <summary>
    /// The date and time the meal was created.
    /// </summary>
    public required DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// The list of ingredients associated with the meal.
    /// </summary>
    public virtual ICollection<MealIngredient> MealIngredients { get; set; } = [];
}
