namespace GlucoPilot.Mail;

public interface IMailService
{
    Task SendAsync(MailRequest request, CancellationToken cancellationToken = default);
}