using GlucoPilot.Data.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace GlucoPilot.Data.Entities;

[ExcludeFromCodeCoverage]
[Table("pens")]
public class Pen
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The unique identifier for the user who added the pen.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The user who added the pen.
    /// </summary>
    public virtual User? User { get; set; }

    /// <summary>
    /// The unique identifier for the insulin associated with the pen.
    /// </summary>
    public required Guid InsulinId { get; set; }

    /// <summary>
    /// The insulin associated with the pen.
    /// </summary>
    public virtual Insulin? Insulin { get; set; }

    /// <summary>
    /// The date and time the pen was added.
    /// </summary>
    public required DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// The date and time the pen was last updated.
    /// </summary>
    public DateTimeOffset? Updated { get; set; }

    /// <summary>
    /// The model of the smart pen.
    /// </summary>
    public required PenModel Model { get; set; }

    /// <summary>
    /// The colour of the smart pen. This is for the end user to easily recognise which pen is which.
    /// </summary>
    public required PenColour Colour { get; set; }

    /// <summary>
    /// The serial number of the smart pen.
    /// </summary>
    [MaxLength(100)]
    public required string Serial { get; set; }

    /// <summary>
    /// The start time of the pen's usage.
    /// </summary>
    public DateTimeOffset StartTime { get; set; } = DateTimeOffset.UtcNow;
}
