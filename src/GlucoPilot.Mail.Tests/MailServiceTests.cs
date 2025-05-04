using MailKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Moq;

namespace GlucoPilot.Mail.Tests;

[TestFixture]
public class MailServiceTests
{
    private static readonly string[] ToAddresses = ["to@example.com"];
    private static readonly string[] FromAddresses = ["from@example.com"];
    
    private Mock<ISmtpClientFactory> _smtpClientFactory;
    private Mock<ISmtpClient> _smtpClient;
    private Mock<IOptions<MailOptions>> _options;
    private MailOptions _mailOptions;
    private Mock<ILogger<MailService>> _logger;

    private MailService _sut;

    [SetUp]
    public void Setup()
    {
        _smtpClientFactory = new Mock<ISmtpClientFactory>();
        _smtpClient = new Mock<ISmtpClient>();
        _smtpClientFactory.Setup(x => x.Create()).Returns(_smtpClient.Object);
        _options = new Mock<IOptions<MailOptions>>();
        _mailOptions = new MailOptions();
        _options.Setup(x => x.Value).Returns(_mailOptions);
        _logger = new Mock<ILogger<MailService>>();
        _logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        _sut = new MailService(_smtpClientFactory.Object, _options.Object, _logger.Object);
    }

    [Test]
    public void Constructor_Throws()
    {
        Assert.Multiple(() =>
        {
            Assert.That(() => new MailService(null!, _options.Object, _logger.Object), Throws.ArgumentNullException);
            Assert.That(() => new MailService(_smtpClientFactory.Object, null!, _logger.Object),
                Throws.ArgumentNullException);
            Assert.That(() => new MailService(_smtpClientFactory.Object, _options.Object, null!),
                Throws.ArgumentNullException);
        });
    }

    [Test]
    public async Task SendAsync_WithValidRequest_SendsEmailSuccessfully()
    {
        var request = new MailRequest
        {
            To = new List<string> { "to@example.com" },
            From = "from@example.com",
            Subject = "Test Subject",
            Body = "Test Body"
        };

        await _sut.SendAsync(request);

        _smtpClient.Verify(
            client => client.SendAsync(It.Is<MimeMessage>(m =>
                    m.To.Select(x => x.ToString()).SequenceEqual(ToAddresses) &&
                    m.From.Select(x => x.ToString()).SequenceEqual(FromAddresses) &&
                    m.Subject == "Test Subject" && m.HtmlBody == "Test Body"), It.IsAny<CancellationToken>(),
                It.IsAny<ITransferProgress>()), Times.Once);
    }

    [Test]
    public async Task SendAsync_WithSmtpException_LogsError()
    {
        var request = new MailRequest
        {
            To = new List<string> { "to@example.com" },
            Subject = "Test Subject",
            Body = "Test Body"
        };

        _smtpClient.Setup(client =>
                client.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>(), It.IsAny<ITransferProgress>()))
            .ThrowsAsync(new SmtpCommandException(SmtpErrorCode.RecipientNotAccepted, SmtpStatusCode.MailboxUnavailable,
                "Mailbox unavailable"));

        await _sut.SendAsync(request);

        _logger.Verify(logger => logger.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SMTP message failed to send")),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
    }
}