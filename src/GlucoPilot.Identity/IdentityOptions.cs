using System.ComponentModel.DataAnnotations;

namespace GlucoPilot.Identity;

public sealed class IdentityOptions
{
    [Required]
    public required string TokenSigningKey { get; init; }
    
    [Required]
    public required int TokenExpirationInMinutes { get; init; }
}