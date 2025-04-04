using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlucoPilot.Data.Entities;

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
    /// The name of the meal.
    /// </summary>
    public required string Name { get; set; } = string.Empty;

    /// <summary>
    /// The date and time the meal was created.
    /// </summary>
    public required DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// The list of ingredients in the meal.
    /// </summary>
    public List<Ingredient> Ingredients { get; set; } = [];
}
