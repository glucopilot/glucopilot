namespace GlucoPilot.Identity.Models;

public sealed record LoginResponse
{
    public required string Token { get; init; }
}