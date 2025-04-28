using GlucoPilot.Api.Models;
using System;
using System.Collections.Generic;

namespace GlucoPilot.Api.Endpoints.Insulins.List;

public sealed record ListInsulinsResponse : PagedResponse
{
    public required ICollection<GetInsulinResponse> Insulins { get; init; } = [];
}

public sealed record GetInsulinResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required InsulinType Type { get; init; }
    public double? Duration { get; init; }
    public double? Scale { get; init; }
    public double? PeakTime { get; init; }
    public required DateTimeOffset Created { get; init; }
    public DateTimeOffset? Updated { get; init; }
}
