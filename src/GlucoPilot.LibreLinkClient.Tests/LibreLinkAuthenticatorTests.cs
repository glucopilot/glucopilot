using System.Net;
using System.Text.Json;
using GlucoPilot.LibreLinkClient.Exceptions;
using GlucoPilot.LibreLinkClient.Models;
using Moq;
using Moq.Protected;

namespace GlucoPilot.LibreLinkClient.Tests;

[TestFixture]
public sealed class LibreLinkAuthenticatorTests
{
    private Mock<HttpMessageHandler> _httpMessageHandler;
    private HttpClient _httpClient;

    private LibreLinkAuthenticator _sut;

    [SetUp]
    public void SetUp()
    {
        _httpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api.librelink.com"),
        };

        _sut = new LibreLinkAuthenticator(_httpClient);
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient.Dispose();
    }

    [Test]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthTicket()
    {
        var authTicket = new AuthTicket
        { Token = "valid_token", Expires = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds() };
        var loginResponse = new LibreLinkResponse<LoginResponse>
        {
            Data = new LoginResponse { AuthTicket = authTicket }
        };
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(loginResponse))
        };

        _httpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        var result = await _sut.LoginAsync("username", "password");

        Assert.That(result, Is.Not.Null);
        Assert.That(result.AuthTicket.Token, Is.EqualTo(authTicket.Token));
    }

    [Test]
    public void LoginAsync_InvalidCredentials_ThrowsException()
    {
        var responseMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized);

        _httpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        Assert.That(() => _sut.LoginAsync("username", "password"),
            Throws.InstanceOf<LibreLinkAuthenticationFailedException>());
    }

    [Test]
    public void LoginAsync_ExpiredAuthTicket_ThrowsException()
    {
        var expiredTicket = new AuthTicket
        { Token = "expired_token", Expires = DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeSeconds() };

        Assert.That(() => _sut.LoginAsync(expiredTicket), Throws.InstanceOf<LibreLinkAuthenticationExpiredException>());
    }

    [Test]
    public async Task IsAuthenticated_ValidAuthTicket_ReturnsTrue()
    {
        var authTicket = new AuthTicket
        { Token = "valid_token", Expires = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds() };
        await _sut.LoginAsync(authTicket);

        var isAuthenticated = _sut.IsAuthenticated;

        Assert.That(isAuthenticated, Is.True);
    }

    [Test]
    public void IsAuthenticated_NoAuthTicket_ReturnsFalse()
    {
        var isAuthenticated = _sut.IsAuthenticated;

        Assert.That(isAuthenticated, Is.False);
    }
}