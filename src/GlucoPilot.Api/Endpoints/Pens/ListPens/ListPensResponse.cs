using GlucoPilot.Api.Models;
using System;
using System.Collections;
using System.Collections.Generic;

namespace GlucoPilot.Api.Endpoints.Pens.ListPens
{
    internal sealed record ListPensResponse : PagedResponse
    {
        public required ICollection<PenResponse> Pens { get; set; }
    }

    internal sealed record PenResponse
    {
        public Guid Id { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }
        public PenModel Model { get; set; }
        public PenColour Colour { get; set; }
        public required string Serial { get; set; }
        public Guid InsulinId { get; set; }
        public required string InsulinName { get; set; }
        public DateTimeOffset StartTime { get; set; }
    }
}