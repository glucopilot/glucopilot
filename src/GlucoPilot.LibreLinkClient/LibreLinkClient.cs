using System.Net.Http.Headers;
using System.Text.Json;
using GlucoPilot.LibreLinkClient.Exceptions;
using GlucoPilot.LibreLinkClient.Models;

namespace GlucoPilot.LibreLinkClient;

internal sealed class LibreLinkClient : ILibreLinkClient
{
    private readonly HttpClient _httpClient;
    private readonly ILibreLinkAuthenticator _libreLinkAuthenticator;

    public LibreLinkClient(HttpClient httpClient, ILibreLinkAuthenticator libreLinkAuthenticator)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _libreLinkAuthenticator =
            libreLinkAuthenticator ?? throw new ArgumentNullException(nameof(libreLinkAuthenticator));
    }

    public async Task<IReadOnlyCollection<ConnectionData>> GetConnectionsAsync(
        CancellationToken cancellationToken = default)
    {
        ValidateAuth();

        var request = new HttpRequestMessage(HttpMethod.Get, "/llu/connections");
        var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        var result = await JsonSerializer.DeserializeAsync<ConnectionResponse>(
            await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false),
            cancellationToken: cancellationToken).ConfigureAwait(false);

        return result?.Data ?? [];
    }

    public async Task<AuthTicket> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var result = await _libreLinkAuthenticator.LoginAsync(username, password, cancellationToken)
            .ConfigureAwait(false);
        if (!string.IsNullOrWhiteSpace(result.AuthTicket.Token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", result.AuthTicket.Token);
        }

        return result.AuthTicket;
    }

    public async Task LoginAsync(AuthTicket authTicket, CancellationToken cancellationToken = default)
    {
        var result = await _libreLinkAuthenticator.LoginAsync(authTicket, cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(result.AuthTicket.Token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", result.AuthTicket.Token);
        }
    }

    public async Task<GraphInformation?> GraphAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        ValidateAuth();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/llu/connections/{patientId}/graph");
        var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

        var result = await JsonSerializer.DeserializeAsync<GraphResponse>(
            await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false),
            cancellationToken: cancellationToken).ConfigureAwait(false);

        return result?.Data;
    }

    private void ValidateAuth()
    {
        if (_libreLinkAuthenticator.IsAuthenticated)
        {
            return;
        }

        if (_httpClient.DefaultRequestHeaders.Authorization is not null)
        {
            // If the auth header is set and the user is not authenticated, it means the auth ticket is expired.
            throw new LibreLinkAuthenticationExpiredException();
        }

        throw new LibreLinkNotAuthenticatedException();
    }
}