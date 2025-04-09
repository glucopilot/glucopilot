using System.Text.Json.Serialization;

namespace GlucoPilot.LibreLinkClient.Models
{
    public sealed record GraphData
    {
        [JsonPropertyName("FactoryTimestamp")]
        public required string FactoryTimeStamp { get; init; }

        [JsonPropertyName("Timestamp")]
        public required string TimeStamp { get; init; }

        [JsonPropertyName("Value")]
        public double Value { get; init; }

        [JsonPropertyName("TrendArrow")]
        public int TrendArrow { get; init; }
    }
}
