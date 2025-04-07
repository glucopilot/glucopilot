using MailKit.Net.Smtp;

namespace GlucoPilot.Mail;

internal sealed class SmtpClientFactory : ISmtpClientFactory
{
    public ISmtpClient Create()
    {
        return new SmtpClient();
    }
}