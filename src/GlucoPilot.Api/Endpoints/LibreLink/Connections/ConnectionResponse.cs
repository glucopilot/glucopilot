using System;

namespace GlucoPilot.Api.Endpoints.LibreLink;

public record ConnectionResponse
{
    public Guid PatientId { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
}
