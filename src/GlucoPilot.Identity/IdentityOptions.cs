using System.ComponentModel.DataAnnotations;

namespace GlucoPilot.Identity;

public sealed class IdentityOptions
{
    [Required] public string TokenSigningKey { get; init; } = "";

    [Required] public int TokenExpirationInMinutes { get; init; } = 60;

    [Required] public string RefreshTokenCookieName { get; set; } = "";
    
    [Required] public int RefreshTokenExpirationInDays { get; set; } = 30;

    public bool RequireEmailVerification { get; set; } = true;
}