using GlucoPilot.Data.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlucoPilot.Data.Entities;

/// <summary>
/// Represents a patient (a user with diabetes) in the system.
/// </summary>
[Table("users")]
public class Patient : User
{
    /// <summary>
    /// The authentication ticket to the patient's diabetes sensor data.
    /// </summary>
    public AuthTicket? AuthTicket { get; set; }

    /// <summary>
    /// The unique identifier for the patient in the diabetes sensor system.
    /// </summary>
    public string? PatientId { get; set; }

    /// <summary>
    /// The diabetes sensor system that the patient is using.
    /// </summary>
    public GlucoseProvider GlucoseProvider { get; set; } = GlucoseProvider.None;
}