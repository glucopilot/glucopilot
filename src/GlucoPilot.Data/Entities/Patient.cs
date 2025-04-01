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
}