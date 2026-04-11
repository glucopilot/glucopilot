using System;
using System.Text.Json.Serialization;

namespace GlucoPilot.LibreLinkClient.Models;

public sealed record LoginResponse
{
    [JsonPropertyName("authTicket")]
    public AuthTicket? AuthTicket { get; init; }

    [JsonPropertyName("step")]
    public LoginTerms.LoginTermsStep? Step { get; init; }

    [JsonPropertyName("user")]
    public UserData? UserData { get; init; }

    [JsonPropertyName("redirect")]
    public bool Redirect { get; init; } = false;

    [JsonPropertyName("region")]
    public string? RegionCode { get; init; }

    [JsonIgnore]
    public LibreRegion? Region => Enum.TryParse<LibreRegion>(RegionCode, ignoreCase: true, out var region) ? region : null;
}