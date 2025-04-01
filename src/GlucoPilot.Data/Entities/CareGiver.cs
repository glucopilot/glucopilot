using System.ComponentModel.DataAnnotations.Schema;

namespace GlucoPilot.Data.Entities;

/// <summary>
/// Represents a caregiver (a user who can view patient data) in the system.
/// </summary>
[Table("users")]
public class CareGiver : User
{
}