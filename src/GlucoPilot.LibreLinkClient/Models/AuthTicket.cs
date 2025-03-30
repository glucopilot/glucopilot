using System.Text.Json.Serialization;

namespace GlucoPilot.LibreLinkClient.Models;

public sealed record AuthTicket
{
    [JsonPropertyName("token")]
    public required string Token { get; init; }

    [JsonPropertyName("expires")]
    public long Expires { get; init; }

    [JsonPropertyName("duration")]
    public long Duration { get; init; }
}