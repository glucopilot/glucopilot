using System.ComponentModel.DataAnnotations;

namespace GlucoPilot.Identity;

public sealed class IdentityOptions
{
    [Required] public string TokenSigningKey { get; init; } = "";

    [Required] public int TokenExpirationInMinutes { get; init; } = 60;

    public bool RequireEmailVerification { get; set; } = true;
}