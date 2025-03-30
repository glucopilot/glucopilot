using System.Text.Json.Serialization;

namespace GlucoPilot.LibreLinkClient.Models;

public sealed record LoginRedirect
{
    [JsonPropertyName("redirect")]
    public bool Redirect { get; init; }

    [JsonPropertyName("region")]
    public string? RegionCode { get; init; }

    [JsonIgnore]
    public LibreRegion? Region => Enum.TryParse<LibreRegion>(RegionCode, out var region) ? region : null;
}