using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace GlucoPilot.Data.Entities;

/// <summary>
/// Sensor is details about a sensor used by a user. It holds the 
/// </summary>
[ExcludeFromCodeCoverage]
[Table("Sensor")]
public class Sensor
{
    /// <summary>
    /// Unique identifier for the sensor.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The id of the user who created the sensor.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// The user who created the sensor.
    /// </summary>
    public virtual User? User { get; set; }

    /// <summary>
    /// The date and time the sensor was added to user.
    /// </summary>
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// The id of the sensor.
    /// </summary>
    public required string SensorId { get; set; }

    /// <summary>
    /// The date and time the sensor was started.
    /// </summary>
    public required DateTimeOffset Started { get; set; }

    /// <summary>
    /// The date and time the sensor expired.
    /// </summary>
    public required DateTimeOffset Expires { get; set; }
}
