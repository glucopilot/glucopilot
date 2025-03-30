using System.Text.Json.Serialization;

namespace GlucoPilot.LibreLinkClient.Models;

public record LoginTerms
{
    [JsonPropertyName("step")]
    public required LoginTermsStep Step { get; init; }
    
    [JsonPropertyName("user")]
    public required LoginTermsUser User { get; init; }
    
    [JsonPropertyName("authTicket")]
    public required AuthTicket AuthTicket { get; init; }

    public record LoginTermsStep
    {
        [JsonPropertyName("status")]
        public int Status { get; init; }
    
        // TODO maybe make a TOU enum
        [JsonPropertyName("type")]
        public required string Type { get; init; }
    
        [JsonPropertyName("componentName")]
        public required string ComponentName { get; init; }

        [JsonPropertyName("props")]
        public required LoginTermsProperties Properties { get; init; }
        
        public record LoginTermsProperties
        {
            [JsonPropertyName("reaccept")]
            public bool ReAccept { get; init; }
        
            [JsonPropertyName("titleKey")]
            public required string TitleKey { get; init; }
        
            // TODO maybe make a TOU enum
            [JsonPropertyName("type")]
            public required string Type { get; init; }
        }
    }

    public record LoginTermsUser
    {
        [JsonPropertyName("accountType")]
        public required string AccountType { get; init; }
        
        [JsonPropertyName("country")]
        public required string Country { get; init; }
        
        [JsonPropertyName("uiLanguage")]
        public required string UiLanguage { get; init; }
    }
}