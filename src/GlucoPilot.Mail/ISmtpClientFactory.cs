using MailKit.Net.Smtp;

namespace GlucoPilot.Mail;

internal interface ISmtpClientFactory
{
    ISmtpClient Create();
}