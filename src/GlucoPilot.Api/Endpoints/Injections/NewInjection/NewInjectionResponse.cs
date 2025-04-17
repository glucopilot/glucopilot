using System;

namespace GlucoPilot.Api.Endpoints.Injections.NewInjection;

public record NewInjectionResponse
{
    public required Guid Id { get; init; }
    public required DateTimeOffset Created { get; init; }
    public required Guid InsulinId { get; init; }
    public required double Units { get; init; }
    public DateTimeOffset? Updated { get; init; }
}