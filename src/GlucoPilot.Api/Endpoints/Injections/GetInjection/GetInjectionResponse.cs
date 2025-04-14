using System;

namespace GlucoPilot.Api.Endpoints.Injections.GetInjection
{
    internal class GetInjectionResponse
    {
        public required Guid Id { get; set; }
        public required DateTimeOffset Created { get; set; }
        public required Guid InsulinId { get; set; }
        public required string InsulinName { get; set; }
        public required double Units { get; set; }
    }
}