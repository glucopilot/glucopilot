using GlucoPilot.Data.Enums;
using System;

namespace GlucoPilot.Api.Endpoints.Insulins.NewInsulin
{
    public class NewInsulinResponse
    {
        public required Guid Id { get; init; }
        public required string Name { get; init; }
        public required InsulinType Type { get; init; }
        public double? Duration { get; init; }
        public double? Scale { get; init; }
        public double? PeakTime { get; init; }
    }
}
