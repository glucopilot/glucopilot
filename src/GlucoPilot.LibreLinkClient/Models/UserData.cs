using System;
using System.Text.Json.Serialization;

namespace GlucoPilot.LibreLinkClient.Models;

public record UserData
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("firstName")]
    public string? FirstName { get; init; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; init; }

    [JsonPropertyName("dateOfBirth")]
    public long UnixDateOfBirth { get; init; }

    [JsonIgnore]
    public DateTimeOffset DateOfBirth => DateTimeOffset.FromUnixTimeSeconds(UnixDateOfBirth);

    [JsonPropertyName("email")]
    public string? Email { get; init; }

    [JsonPropertyName("country")]
    public required string Country { get; init; }

    [JsonPropertyName("uiLanguage")]
    public required string UiLanguage { get; init; }

    [JsonPropertyName("communicationLanguage")]
    public string? CommunicationLanguage { get; init; }

    [JsonPropertyName("accountType")]
    public required string AccountType { get; init; }

    [JsonPropertyName("uom")]
    public string? Uom { get; init; }

    [JsonPropertyName("dateFormat")]
    public string? DateFormat { get; init; }

    [JsonPropertyName("emailDay")]
    public int[]? EmailDay { get; init; }
}