using System;

namespace GlucoPilot.Api.Endpoints.LibreLink.PairConnection;

public sealed record PairConnectionResponse
{
    public required Guid Id { get; init; }
    public required Guid PatientId { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
}
