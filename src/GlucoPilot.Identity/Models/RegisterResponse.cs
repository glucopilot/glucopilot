using System;

namespace GlucoPilot.Identity.Models;

public sealed record RegisterResponse
{
    public Guid Id { get; init; }
    public required string Email { get; init; }
    public DateTimeOffset Created { get; init; }
    public DateTimeOffset? Updated { get; init; }
    public bool AcceptedTerms { get; init; }
}