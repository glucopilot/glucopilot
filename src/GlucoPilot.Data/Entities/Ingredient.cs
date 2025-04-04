using GlucoPilot.Data.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlucoPilot.Data.Entities;

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
    /// The unit of measurement of he ingredient that the nutritional values are based on.
    /// </summary>
    public required UnitOfMeasurement Uom { get; set; }
}
