using System;
using GlucoPilot.Identity.Extensions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace GlucoPilot.Identity.Tests.Extensions;

[TestFixture]
internal sealed class HttpContextExtensionTests
{
    [Test]
    public void IpAddress_With_X_Forwarded_For_Header_Returns_Correct_Ip()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Forwarded-For"] = "192.168.1.1";

        var ipAddress = context.IpAddress();

        Assert.That(ipAddress, Is.EqualTo("192.168.1.1"));
    }

    [Test]
    public void IpAddress_Without_X_Forwarded_For_Header_Returns_Remote_Ip()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("10.0.0.1");

        var ipAddress = context.IpAddress();

        Assert.That(ipAddress, Is.EqualTo("10.0.0.1"));
    }

    [Test]
    public void IpAddress_Without_X_Forwarded_For_And_Remote_Ip_Returns_Empty_String()
    {
        var context = new DefaultHttpContext();

        var ipAddress = context.IpAddress();

        Assert.That(ipAddress, Is.EqualTo(string.Empty));
    }

    [Test]
    public void SetRefreshTokenCookie_Sets_Cookie_With_Correct_Options()
    {
        var cookiesMock = new Mock<IResponseCookies>();
        var responseMock = new Mock<HttpResponse>();
        responseMock.SetupGet(r => r.Cookies).Returns(cookiesMock.Object);

        var contextMock = new Mock<HttpContext>();
        contextMock.SetupGet(c => c.Response).Returns(responseMock.Object);

        var options = new IdentityOptions
        {
            RefreshTokenCookieName = "RefreshToken",
            RefreshTokenExpirationInDays = 7
        };

        contextMock.Object.SetRefreshTokenCookie("test-refresh-token", options);

        cookiesMock.Verify(c => c.Append(
            It.Is<string>(name => name == "RefreshToken"),
            It.Is<string>(value => value == "test-refresh-token"),
            It.Is<CookieOptions>(opts => opts.HttpOnly && opts.Expires > DateTimeOffset.UtcNow)
        ), Times.Once);
    }
}