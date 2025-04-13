using GlucoPilot.Data.Enums;
using System;

namespace GlucoPilot.Api.Endpoints.Insulins.GetInsulin
{
    public sealed record GetInsulinResponse
    {
        public required Guid Id { get; init; }
        public required string Name { get; init; }
        public required InsulinType Type { get; init; }
        public double? Duration { get; init; }
        public double? Scale { get; init; }
        public double? PeakTime { get; init; }
        public required DateTimeOffset Created { get; init; }
    }
}
