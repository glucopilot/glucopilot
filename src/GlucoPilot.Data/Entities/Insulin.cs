using GlucoPilot.Data.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlucoPilot.Data.Entities;

/// <summary>
/// Insulin is the type of insulin used by the user. Different insulins work in different ways.
/// Duration, scale and peak time represent this profile.
/// </summary>
[Table("insulin")]
public class Insulin
{
    /// <summary>
    /// Unique identifier for the insulin.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The id of the user who created the insulin.
    /// </summary>
    public Guid? UserId { get; set; } = null;

    /// <summary>
    /// The date and time the insulin was created.
    /// </summary>
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// The name of the insulin.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The type of insulin (Bolus or Basal).
    /// </summary>
    public required InsulinType Type { get; set; }

    /// <summary>
    /// The length of time (in hours) the insulin is active.
    /// </summary>
    public double? Duration { get; set; }

    /// <summary>
    /// The spread of the curve for the insulin action.
    /// Short acting insulin has a sharper curve, while long acting insulin has a flatter curve.
    /// </summary>
    public double? Scale { get; set; }

    /// <summary>
    /// The time (in hours) it takes for the insulin to reach its peak effect.
    /// </summary>
    public double? PeakTime { get; set; }
}
