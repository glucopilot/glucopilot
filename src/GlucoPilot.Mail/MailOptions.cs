namespace GlucoPilot.Mail;

public sealed class MailOptions
{
    public string? SmtpHost { get; init; }
    public int SmtpPort { get; init; } = 587;
    public string? SmtpUser { get; init; }
    public string? SmtpPassword { get; init; }
    public string? DisplayName { get; init; }
    public string? From { get; init; }
}