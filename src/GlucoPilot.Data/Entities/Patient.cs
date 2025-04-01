using System.ComponentModel.DataAnnotations.Schema;

namespace GlucoPilot.Data.Entities;

[Table("users")]
public class Patient : User
{
    public AuthTicket? AuthTicket { get; set; }
}