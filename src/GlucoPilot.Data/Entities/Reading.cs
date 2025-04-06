using GlucoPilot.Data.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlucoPilot.Data.Entities;

/// <summary>
/// A reading is a glucose measurement taken by sensor used by the user.
/// </summary>
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
    /// The id of the user who created the reading.
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// The user who created the reading.
    /// </summary>
    public virtual User? User { get; set; }

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
