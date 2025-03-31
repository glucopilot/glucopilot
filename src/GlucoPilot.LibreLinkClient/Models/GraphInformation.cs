using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GlucoPilot.LibreLinkClient.Models
{
    public sealed record GraphInformation
    {
        [JsonPropertyName("connection")]
        public ConnectionData? Connection { get; init; }

        [JsonPropertyName("graphData")]
        public IReadOnlyCollection<GraphData> GraphData { get; init; } = [];
    }
}
