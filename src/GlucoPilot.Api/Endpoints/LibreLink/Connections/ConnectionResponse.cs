using System;

namespace GlucoPilot.Api.Endpoints.LibreLink.Connections;

public record ConnectionResponse
{
    public Guid PatientId { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
}
