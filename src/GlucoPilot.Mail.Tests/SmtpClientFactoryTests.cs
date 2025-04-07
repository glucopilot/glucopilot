using MailKit.Net.Smtp;

namespace GlucoPilot.Mail.Tests;

[TestFixture]
internal sealed class SmtpClientFactoryTests
{
    private SmtpClientFactory _sut;

    [SetUp]
    public void Setup()
    {
        _sut = new SmtpClientFactory();
    }

    [Test]
    public void Create_ReturnsSmtpClient()
    {
        var result = _sut.Create();

        Assert.That(result, Is.InstanceOf<SmtpClient>());
    }
}