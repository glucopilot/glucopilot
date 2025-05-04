using System;

namespace GlucoPilot.Api.Endpoints.Injections.UpdateInjection;

public record UpdateInjectionResponse
{
    public required Guid Id { get; init; }
    public required Guid InsulinId { get; init; }
    public required string InsulinName { get; init; }
    public required double Units { get; init; }
    public DateTimeOffset? Updated { get; init; }
}