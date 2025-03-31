using System;
using System.Text.Json.Serialization;

namespace GlucoPilot.LibreLinkClient.Models;

public record UserData
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("firstName")]
    public required string FirstName { get; init; }

    [JsonPropertyName("lastName")]
    public required string LastName { get; init; }

    [JsonPropertyName("dateOfBirth")]
    public required long UnixDateOfBirth { get; init; }

    // TODO confirm if this is actually a unix timestamp
    [JsonIgnore]
    public DateTime DateOfBirth => DateTime.MinValue.AddMilliseconds(UnixDateOfBirth);

    [JsonPropertyName("email")]
    public required string Email { get; init; }

    [JsonPropertyName("country")]
    public required string Country { get; init; }

    [JsonPropertyName("uiLanguage")]
    public required string UiLanguage { get; init; }

    [JsonPropertyName("communicationLanguage")]
    public required string CommunicationLanguage { get; init; }

    [JsonPropertyName("accountType")]
    public required string AccountType { get; init; }

    [JsonPropertyName("uom")]
    public required string Uom { get; init; }

    [JsonPropertyName("dateFormat")]
    public required string DateFormat { get; init; }

    [JsonPropertyName("emailDay")]
    public required int[] EmailDay { get; init; }
}