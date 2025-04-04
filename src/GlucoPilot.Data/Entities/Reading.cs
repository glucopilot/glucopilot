using GlucoPilot.Data.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlucoPilot.Data.Entities;

[Table("readings")]
public class Reading
{
    /// <summary>
    /// The unique identifier for the reading.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The date and time the reading was taken.
    /// </summary>
    public required DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// The value of the glucose reading. Stored in gl/dl. Divide by 18 to get mmol/L.
    /// </summary>
    public required double GlucoseLevel { get; set; }

    /// <summary>
    /// A value representing the trend of the glucose level.
    /// </summary>
    public required ReadingDirection Direction { get; set; }
}
