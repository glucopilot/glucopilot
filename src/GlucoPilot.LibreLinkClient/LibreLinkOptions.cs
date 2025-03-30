namespace GlucoPilot.LibreLinkClient;

public sealed class LibreLinkOptions
{
    internal const string LinkUpVersion = "4.7.0";
    internal const string LinkUpProduct = "llu.ios";

    public LibreRegion Region { get; set; }
    public string UserAgent { get; set; } = "Mozilla/5.0 (iPhone; CPU iPhone OS 16_5_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.5 Mobile/15E148 Safari/604.1";

    public string ApiUri => $"https://api-{Region.ToString().ToLowerInvariant()}.libreview.io";
}