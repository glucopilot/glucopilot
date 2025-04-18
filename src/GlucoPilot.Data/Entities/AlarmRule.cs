using GlucoPilot.Data.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlucoPilot.Data.Entities;

[Table("alarm_rule")]
public class AlarmRule
{
    /// <summary>
    /// The unique identifier for the alarm rule.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The unique identifier for the user associated with the alarm rule.
    /// </summary>
    public required Guid PatientId { get; set; }

    /// <summary>
    /// The patient associated with the alarm rule.
    /// </summary>
    public virtual Patient? Patient { get; set; }

    /// <summary>
    /// The value at which the alarm will trigger.
    /// </summary>
    public required int TargetValue { get; set; }

    /// <summary>
    /// The condition for the alarm to trigger, such as greater than, less than, compared to TargetValue.
    /// </summary>
    public required AlarmTargetDirection TargetDirection { get; set; }
}
