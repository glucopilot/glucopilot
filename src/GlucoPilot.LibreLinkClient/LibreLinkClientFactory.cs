using System;
using System.Net.Http;
using Microsoft.Extensions.Options;

namespace GlucoPilot.LibreLinkClient;

public sealed class LibreLinkClientFactory : ILibreLinkClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<LibreLinkOptions> _options;

    public LibreLinkClientFactory(IHttpClientFactory httpClientFactory, IOptions<LibreLinkOptions> options)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _options = options;
    }
    
    public ILibreLinkClient CreateLibreLinkClient(LibreRegion region)
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri(LibreLinkOptions.ApiUri(region));
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Add("User-Agent", _options.Value.UserAgent);
        httpClient.DefaultRequestHeaders.Add("version", _options.Value.Version);
        httpClient.DefaultRequestHeaders.Add("product", _options.Value.Product);
        
        var authenticator = new LibreLinkAuthenticator(httpClient);
        return new LibreLinkClient(httpClient, authenticator);
    }
}