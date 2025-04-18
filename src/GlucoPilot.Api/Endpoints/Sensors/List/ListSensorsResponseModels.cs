using GlucoPilot.Api.Models;
using System;
using System.Collections.Generic;

namespace GlucoPilot.Api.Endpoints.Sensors.List;

public record ListSensorsResponse : PagedResponse
{
    public required ICollection<ListSensorResponse> Sensors { get; set; }
}

public record ListSensorResponse
{
    public required Guid Id { get; set; }
    public required string SensorId { get; set; }
    public required DateTimeOffset Started { get; set; }
    public required DateTimeOffset Expires { get; set; }
}