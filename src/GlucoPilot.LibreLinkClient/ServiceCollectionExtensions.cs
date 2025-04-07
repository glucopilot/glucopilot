using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;

namespace GlucoPilot.LibreLinkClient;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IHttpClientBuilder AddLibreLinkClient(
        this IServiceCollection services,
        Action<LibreLinkOptions>? configure,
        Action<IHttpClientBuilder>? configureAuthClient = null)
    {
        var authClient = services.AddHttpClient<ILibreLinkAuthenticator, LibreLinkAuthenticator>("LibreLinkAuth")
            .ConfigureHttpClient((_, client) =>
            {
                var options = new LibreLinkOptions();
                configure?.Invoke(options);

                client.BaseAddress = new Uri(options.ApiUri);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("User-Agent", options.UserAgent);
                client.DefaultRequestHeaders.Add("version", LibreLinkOptions.LinkUpVersion);
                client.DefaultRequestHeaders.Add("product", LibreLinkOptions.LinkUpProduct);
            });
        configureAuthClient?.Invoke(authClient);

        return services.AddHttpClient<ILibreLinkClient, LibreLinkClient>()
            .ConfigureHttpClient((_, client) =>
            {
                var options = new LibreLinkOptions();
                configure?.Invoke(options);

                client.BaseAddress = new Uri(options.ApiUri);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("User-Agent", options.UserAgent);
                client.DefaultRequestHeaders.Add("version", LibreLinkOptions.LinkUpVersion);
                client.DefaultRequestHeaders.Add("product", LibreLinkOptions.LinkUpProduct);
            });
    }
}