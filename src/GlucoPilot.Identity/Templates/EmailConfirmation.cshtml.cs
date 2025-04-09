namespace GlucoPilot.Identity.Templates;

public sealed class EmailConfirmation
{
    public required string Email { get; init; }

    public required string Url { get; init; }
}