using System.Text.Json.Serialization;

namespace GlucoPilot.LibreLinkClient.Models;

public record LibreLinkResponse<TModel>
{
    [JsonPropertyName("status")]
    public long Status { get; set; }
    
    [JsonPropertyName("data")]
    public TModel? Data { get; set; }
}