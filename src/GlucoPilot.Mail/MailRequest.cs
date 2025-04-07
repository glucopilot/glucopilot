namespace GlucoPilot.Mail;

public sealed class MailRequest
{
    public IReadOnlyCollection<string> To { get; init; } = Array.Empty<string>();
    public required string Subject { get; init; }
    public string? Body { get; init; }
    public string? From { get; init; }
    public string? DisplayName { get; init; }
    public string? ReplyTo { get; init; }
    public string? ReplyToName { get; init; }
    public IReadOnlyCollection<string> Cc { get; init; } = Array.Empty<string>();
    public IReadOnlyCollection<string> Bcc { get; init; } = Array.Empty<string>();
    public IReadOnlyDictionary<string, byte[]> Attachments { get; init; } = new Dictionary<string, byte[]>();
    public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();
}