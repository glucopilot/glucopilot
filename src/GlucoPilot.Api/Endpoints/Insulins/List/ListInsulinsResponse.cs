using GlucoPilot.Api.Endpoints.Insulins.GetInsulin;
using GlucoPilot.Api.Models;
using System.Collections.Generic;

namespace GlucoPilot.Api.Endpoints.Insulins.List;

public sealed record ListInsulinsResponse : PagedResponse
{
    public required ICollection<GetInsulinResponse> Insulins { get; init; } = [];
}
