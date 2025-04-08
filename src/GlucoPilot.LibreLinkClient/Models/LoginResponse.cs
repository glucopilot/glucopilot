using System.Text.Json.Serialization;

namespace GlucoPilot.LibreLinkClient.Models;

public sealed record LoginResponse
{
    [JsonPropertyName("authTicket")]
    public required AuthTicket AuthTicket { get; init; }

    [JsonPropertyName("step")]
    public LoginTerms.LoginTermsStep? Step { get; init; }

    [JsonPropertyName("user")]
    public required UserData UserData { get; init; }
}