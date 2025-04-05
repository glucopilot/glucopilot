using GlucoPilot.Data.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlucoPilot.Data.Entities;

/// <summary>
/// A treatment represents a medical intervention related to diabetes management.
/// This can be eating a meal, a correction of insulin or carbs or an injection.
/// </summary>
[Table("treatments")]
public class Treatment
{
    /// <summary>
    /// Unique identifier for the treatment.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The id of the user who created the treatment.
    /// </summary>
    public required Guid UserId { get; set; }

    /// <summary>
    /// The date and time when the treatment was created.
    /// </summary>
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// The type of treatment, this can be a meal, an injection or a correction.
    /// </summary>
    [NotMapped]
    public TreatmentType Type
    {
        get
        {
            if (MealId is not null && InjectionId is null)
                return TreatmentType.Correction;
            else if (MealId is not null && InjectionId is not null)
                return TreatmentType.Meal;
            else if (MealId is null && InjectionId is not null)
                return TreatmentType.Injection;
            else
                throw new InvalidOperationException("Invalid treatment type");
        }
        set { }
    }

    /// <summary>
    /// The Id of the last reading (within a time frame) that this treatment is associated with.
    /// </summary>
    public Guid? ReadingId { get; set; }

    /// <summary>
    /// The last reading (within a time frame) associated with this treatment.
    /// </summary>
    public virtual Reading? Reading { get; set; }

    /// <summary>
    /// The Id of the meal that this treatment is associated with.
    /// </summary>
    public Guid? MealId { get; set; }

    /// <summary>
    /// The meal associated with this treatment.
    /// </summary>
    public virtual Meal? Meal { get; set; }

    /// <summary>
    /// The Id of the injection that this treatment is associated with.
    /// </summary>
    public Guid? InjectionId { get; set; }

    /// <summary>
    /// The injection associated with this treatment.
    /// </summary>
    public virtual Injection? Injection { get; set; }
}
