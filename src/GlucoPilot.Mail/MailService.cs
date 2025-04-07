using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace GlucoPilot.Mail;

internal partial class MailService : IMailService
{
    private readonly ISmtpClientFactory _smtpClientFactory;
    private readonly MailOptions _options;
    private readonly ILogger<MailService> _logger;

    public MailService(ISmtpClientFactory smtpClientFactory, IOptions<MailOptions> options, ILogger<MailService> logger)
    {
        _smtpClientFactory = smtpClientFactory ?? throw new ArgumentNullException(nameof(smtpClientFactory));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SendAsync(MailRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var email = new MimeMessage();

            email.From.Add(new MailboxAddress(_options.DisplayName, request.From ?? _options.From));

            foreach (var address in request.To)
                email.To.Add(MailboxAddress.Parse(address));

            if (!string.IsNullOrEmpty(request.ReplyTo))
                email.ReplyTo.Add(new MailboxAddress(request.ReplyToName, request.ReplyTo));

            foreach (var address in request.Bcc.Where(bccValue => !string.IsNullOrWhiteSpace(bccValue)))
                email.Bcc.Add(MailboxAddress.Parse(address.Trim()));

            foreach (var address in request.Cc.Where(ccValue => !string.IsNullOrWhiteSpace(ccValue)))
                email.Cc.Add(MailboxAddress.Parse(address.Trim()));

            foreach (var header in request.Headers)
                email.Headers.Add(header.Key, header.Value);

            var builder = new BodyBuilder();
            email.Sender = new MailboxAddress(request.DisplayName ?? _options.DisplayName, request.From ?? _options.From);
            email.Subject = request.Subject;
            builder.HtmlBody = request.Body;

            foreach (var attachmentInfo in request.Attachments)
                builder.Attachments.Add(attachmentInfo.Key, attachmentInfo.Value);

            email.Body = builder.ToMessageBody();

            using var smtp = _smtpClientFactory.Create();
            await smtp.ConnectAsync(_options.SmtpHost, _options.SmtpPort, SecureSocketOptions.StartTls, cancellationToken).ConfigureAwait(false);
            await smtp.AuthenticateAsync(_options.SmtpUser, _options.SmtpPassword, cancellationToken).ConfigureAwait(false);
            await smtp.SendAsync(email, cancellationToken).ConfigureAwait(false);
            await smtp.DisconnectAsync(true, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            var context = new Dictionary<string, object?>()
            {
                { "Recipients", request.To },
                { "Subject", request.Subject },
                { "BodySize", request.Body?.Length ?? 0 },
                { "Attachments", request.Attachments.Count },
                { "StackTrace", ex.StackTrace },
            };
            using var scope = _logger.BeginScope(context);
            SmtpSendFailed(ex);
        }
    }

    [LoggerMessage(LogLevel.Error, "SMTP message failed to send")]
    private partial void SmtpSendFailed(Exception error);
}