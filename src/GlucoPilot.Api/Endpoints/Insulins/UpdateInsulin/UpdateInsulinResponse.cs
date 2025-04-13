using GlucoPilot.Data.Enums;
using System;

namespace GlucoPilot.Api.Endpoints.Insulins.UpdateInsulin;

public record UpdateInsulinResponse
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required InsulinType Type { get; set; }
    public double? Duration { get; set; }
    public double? Scale { get; set; }
    public double? PeakTime { get; set; }
}