using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using GlucoPilot.LibreLinkClient.Exceptions;
using GlucoPilot.LibreLinkClient.Models;
using Moq;
using Moq.Protected;

namespace GlucoPilot.LibreLinkClient.Tests;

[TestFixture]
public class LibreLinkClientTests
{
    private Mock<HttpMessageHandler> _httpMessageHandler;
    private HttpClient _httpClient;
    private Mock<ILibreLinkAuthenticator> _libreLinkAuthenticator;

    private LibreLinkClient _sut;

    [SetUp]
    public void SetUp()
    {
        _httpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api.librelink.com"),
        };
        _libreLinkAuthenticator = new Mock<ILibreLinkAuthenticator>();

        _sut = new LibreLinkClient(_httpClient, _libreLinkAuthenticator.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient.Dispose();
    }

    [Test]
    public async Task GetConnectionsAsync_ValidAuth_ReturnsConnections()
    {
        var connectionData = new List<ConnectionData>
        {
            new ConnectionData
                { Id = Guid.NewGuid(), FirstName = "First", LastName = "Last", PatientId = Guid.NewGuid() }
        };
        var connectionResponse = new ConnectionResponse { Data = connectionData };
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(connectionResponse))
        };
        _httpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        _libreLinkAuthenticator.Setup(a => a.IsAuthenticated).Returns(true);

        var result = await _sut.GetConnectionsAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EquivalentTo(connectionData));
        });
        _libreLinkAuthenticator.Verify(a => a.IsAuthenticated, Times.Once);
    }

    [Test]
    public void GetConnectionsAsync_NoAuth_ThrowsNotAuthenticatedException()
    {
        _libreLinkAuthenticator.Setup(a => a.IsAuthenticated).Returns(false);

        Assert.That(() => _sut.GetConnectionsAsync(), Throws.InstanceOf<LibreLinkNotAuthenticatedException>());
    }

    [Test]
    public void GetConnectionsAsync_ExpiredAuth_ThrowsAuthenticationExpiredException()
    {
        var expiredTicket = new AuthTicket
        { Token = "expired_token", Expires = DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeSeconds() };
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", expiredTicket.Token);

        Assert.That(() => _sut.GetConnectionsAsync(), Throws.InstanceOf<LibreLinkAuthenticationExpiredException>());
    }

    [Test]
    public async Task LoginAsync_ValidCredentials_SetsAuthHeader()
    {
        var authTicket = new AuthTicket
        { Token = "valid_token", Expires = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds() };
        var loginResponse = new LoginResponse { AuthTicket = authTicket };
        _libreLinkAuthenticator.Setup(a =>
                a.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(loginResponse);

        await _sut.LoginAsync("username", "password");

        Assert.Multiple(() =>
        {
            Assert.That(_httpClient.DefaultRequestHeaders.Authorization, Is.Not.Null);
            Assert.That(_httpClient.DefaultRequestHeaders.Authorization.Scheme, Is.EqualTo("Bearer"));
            Assert.That(_httpClient.DefaultRequestHeaders.Authorization.Parameter, Is.EqualTo("valid_token"));
        });
        _libreLinkAuthenticator.Verify(a =>
            a.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task LoginAsync_ValidAuthTicket_SetsAuthHeader()
    {
        var authTicket = new AuthTicket
        { Token = "valid_token", Expires = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds() };
        _libreLinkAuthenticator.Setup(a => a.LoginAsync(It.IsAny<AuthTicket>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LoginResponse { AuthTicket = authTicket });

        await _sut.LoginAsync(authTicket);

        Assert.Multiple(() =>
        {
            Assert.That(_httpClient.DefaultRequestHeaders.Authorization, Is.Not.Null);
            Assert.That(_httpClient.DefaultRequestHeaders.Authorization.Scheme, Is.EqualTo("Bearer"));
            Assert.That(_httpClient.DefaultRequestHeaders.Authorization.Parameter, Is.EqualTo("valid_token"));
        });
        _libreLinkAuthenticator.Verify(a => a.LoginAsync(It.IsAny<AuthTicket>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}