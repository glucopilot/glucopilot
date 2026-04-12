using System;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace GlucoPilot.LibreLinkClient;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IHttpClientBuilder AddLibreLinkClient(
        this IServiceCollection services,
        LibreRegion region,
        Action<LibreLinkOptions>? configure,
        Action<IHttpClientBuilder>? configureAuthClient = null)
    {
        var authClient = services.AddHttpClient<ILibreLinkAuthenticator, LibreLinkAuthenticator>("LibreLinkAuth")
            .ConfigureHttpClient((_, client) =>
            {
                var options = new LibreLinkOptions();
                configure?.Invoke(options);

                client.BaseAddress = new Uri(LibreLinkOptions.ApiUri(region));
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("User-Agent", options.UserAgent);
                client.DefaultRequestHeaders.Add("version", options.Version);
                client.DefaultRequestHeaders.Add("product", options.Product);
            });
        configureAuthClient?.Invoke(authClient);

        return services.AddHttpClient<ILibreLinkClient, LibreLinkClient>()
            .ConfigureHttpClient((_, client) =>
            {
                var options = new LibreLinkOptions();
                configure?.Invoke(options);

                client.BaseAddress = new Uri(LibreLinkOptions.ApiUri(region));
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("User-Agent", options.UserAgent);
                client.DefaultRequestHeaders.Add("version", options.Version);
                client.DefaultRequestHeaders.Add("product", options.Product);
            });
    }

    public static IServiceCollection AddLibreLinkClientFactory(
        this IServiceCollection services)
    {
        return services.AddHttpClient()
            .AddTransient<ILibreLinkClientFactory, LibreLinkClientFactory>();
    }
}