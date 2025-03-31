using System.Text.Json.Serialization;

namespace GlucoPilot.LibreLinkClient.Models
{
    public sealed record GraphData
    {
        [JsonPropertyName("factoryTimeStamp")]
        public string? FactoryTimeStamp { get; init; }

        [JsonPropertyName("timeStamp")]
        public string? TimeStamp { get; init; }

        [JsonPropertyName("value")]
        public double Value { get; init; }

        [JsonPropertyName("trendArrow")]
        public int TrendArrow { get; init; }
    }
}
