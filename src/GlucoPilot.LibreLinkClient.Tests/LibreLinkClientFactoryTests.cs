using Microsoft.Extensions.Options;
using Moq;

namespace GlucoPilot.LibreLinkClient.Tests;

[TestFixture]
internal sealed class LibreLinkClientFactoryTests
{
    private Mock<IHttpClientFactory> _httpClientFactoryMock;
    private LibreLinkOptions _options;
    private Mock<IOptions<LibreLinkOptions>> _libreLinkOptionsMock;
    private Mock<HttpClient> _httpClientMock;
    private LibreLinkClientFactory _sut;

    [SetUp]
    public void SetUp()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _libreLinkOptionsMock = new Mock<IOptions<LibreLinkOptions>>();
        _httpClientMock = new Mock<HttpClient>();
        _options = new LibreLinkOptions();
        _libreLinkOptionsMock.Setup(x => x.Value).Returns(_options);
        _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(_httpClientMock.Object);

        _sut = new LibreLinkClientFactory(_httpClientFactoryMock.Object, _libreLinkOptionsMock.Object);
    }

    [Test]
    public void CreateLibreLinkClient_Returns_LibreLinkClient()
    {
        var result = _sut.CreateLibreLinkClient(LibreRegion.Eu);

        Assert.That(result, Is.TypeOf<LibreLinkClient>());
    }

    [Test]
    public void CreateLibreLinkClient_Returns_LibreLinkClient_With_DefaultOptions()
    {
        _sut.CreateLibreLinkClient(LibreRegion.Eu);

        using var _ = Assert.EnterMultipleScope();
        Assert.That(_httpClientMock.Object.DefaultRequestHeaders.UserAgent.ToString(),
            Is.EqualTo(_options.UserAgent));
        Assert.That(_httpClientMock.Object.DefaultRequestHeaders.GetValues("version").Single(),
            Is.EqualTo(_options.Version));
        Assert.That(_httpClientMock.Object.DefaultRequestHeaders.GetValues("product").Single(),
            Is.EqualTo(_options.Product));
    }

    [Test]
    public void CreateLibreLinkClient_Returns_LibreLinkClient_With_Options()
    {
        _options.UserAgent = "test-user-agent";
        _options.Version = "1.2.3";
        _options.Product = "test-product";

        _sut.CreateLibreLinkClient(LibreRegion.Eu);

        using var _ = Assert.EnterMultipleScope();
        Assert.That(_httpClientMock.Object.DefaultRequestHeaders.UserAgent.ToString(),
            Is.EqualTo(_options.UserAgent));
        Assert.That(_httpClientMock.Object.DefaultRequestHeaders.GetValues("version").Single(),
            Is.EqualTo(_options.Version));
        Assert.That(_httpClientMock.Object.DefaultRequestHeaders.GetValues("product").Single(),
            Is.EqualTo(_options.Product));
    }

    [Test]
    public void CreateLibreLinkClient_Returns_LibreLinkClient_With_For_Region([Values] LibreRegion region)
    {
        _sut.CreateLibreLinkClient(region);

        Assert.That(_httpClientMock.Object.BaseAddress, Is.EqualTo(new Uri(LibreLinkOptions.ApiUri(region))));
    }
}