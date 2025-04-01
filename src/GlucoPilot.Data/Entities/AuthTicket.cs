using Microsoft.EntityFrameworkCore;

namespace GlucoPilot.Data.Entities;

[Owned]
public class AuthTicket
{
    public required string Token { get; set; }
    
    public long Expires { get; set; }
    
    public long Duration { get; set; }
}