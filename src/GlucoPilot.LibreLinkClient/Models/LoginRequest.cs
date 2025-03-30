using System.Text.Json.Serialization;

namespace GlucoPilot.LibreLinkClient.Models;

public record LoginRequest
{
    [JsonPropertyName("email")]
    public required string Email { get; init; }

    [JsonPropertyName("password")]
    public required string Password { get; init; }
}