using System;

namespace GlucoPilot.Identity.Models;

public sealed record LoginResponse
{
    public required string Token { get; init; }
    public Guid UserId { get; init; }
    public required string Email { get; init; }
    public bool IsVerified { get; init; }
}