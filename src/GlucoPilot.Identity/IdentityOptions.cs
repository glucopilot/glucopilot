namespace GlucoPilot.Identity;

public sealed class IdentityOptions
{
    public required string TokenSigningKey { get; init; }
    public required int TokenExpirationInMinutes { get; init; }
}