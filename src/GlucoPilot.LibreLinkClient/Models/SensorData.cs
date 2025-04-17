using System.Text.Json.Serialization;

namespace GlucoPilot.LibreLinkClient.Models;

public sealed record SensorData
{
    [JsonPropertyName("sn")]
    public string SensorId { get; init; } = "";
    [JsonPropertyName("a")]
    public int Started { get; init; }
}
