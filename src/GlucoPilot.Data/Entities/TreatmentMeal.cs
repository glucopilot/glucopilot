using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace GlucoPilot.Data.Entities;

[ExcludeFromCodeCoverage]
[Table("treatment_meal")]
public class TreatmentMeal
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// the unique identifier for the treatment.
    /// </summary>
    public required Guid TreatmentId { get; set; }

    /// <summary>
    /// The treatment the treatment ingredient is associated with.
    /// </summary>
    public virtual Treatment? Treatment { get; set; }

    /// <summary>
    /// The unique identifier for the ingredient.
    /// </summary>
    public required Guid MealId { get; set; }

    /// <summary>
    /// The ingredients the meal ingredient is associated with.
    /// </summary>
    public virtual Meal? Meal { get; set; }

    /// <summary>
    /// The quantity of the ingredient in the meal.
    /// </summary>
    [Range(0, int.MaxValue)]
    public required decimal Quantity { get; set; }
}
