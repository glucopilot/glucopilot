using System;

namespace GlucoPilot.Api.Endpoints.Injections.GetInjection;

public record GetInjectionResponse
{
    public required Guid Id { get; set; }
    public required DateTimeOffset Created { get; set; }
    public required Guid InsulinId { get; set; }
    public required string InsulinName { get; set; }
    public required double Units { get; set; }
    public DateTimeOffset? Updated { get; set; }
}