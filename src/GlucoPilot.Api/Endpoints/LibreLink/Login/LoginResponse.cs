namespace GlucoPilot.Api.Endpoints.LibreLink.Login;

public record LoginResponse
{
    public required string Token { get; init; }
    public required long Expires { get; init; }
    public required long Duration { get; init; }
}
