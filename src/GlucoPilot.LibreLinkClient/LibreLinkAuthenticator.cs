using System.Text;
using System.Text.Json;
using GlucoPilot.LibreLinkClient.Exceptions;
using GlucoPilot.LibreLinkClient.Models;

namespace GlucoPilot.LibreLinkClient;

internal sealed class LibreLinkAuthenticator : ILibreLinkAuthenticator
{
    private readonly HttpClient _httpClient;
    private AuthTicket? _authTicket;

    public LibreLinkAuthenticator(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public bool IsAuthenticated => _authTicket is not null &&
                                   DateTimeOffset.FromUnixTimeSeconds(_authTicket.Expires) > DateTimeOffset.UtcNow;

    public async Task<LoginResponse> LoginAsync(string username, string password,
        CancellationToken cancellationToken = default)
    {
        if (_authTicket is not null &&
            DateTimeOffset.FromUnixTimeMilliseconds(_authTicket.Expires) > DateTimeOffset.UtcNow)
        {
            return new LoginResponse
            {
                AuthTicket = _authTicket,
            };
        }

        var request = new HttpRequestMessage(HttpMethod.Post, "/llu/auth/login")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new LoginRequest { Email = username, Password = password }), Encoding.UTF8,
                "application/json")
        };
        var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new LibreLinkAuthenticationFailedException();
        }

        var result =
            await JsonSerializer.DeserializeAsync<LibreLinkResponse<LoginResponse>>(await response.Content
                .ReadAsStreamAsync(cancellationToken)
                .ConfigureAwait(false), cancellationToken: cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrEmpty(result?.Data?.AuthTicket?.Token))
        {
            throw new LibreLinkAuthenticationFailedException();
        }

        _authTicket = result.Data.AuthTicket;

        return result.Data;
    }

    public Task<LoginResponse> LoginAsync(AuthTicket ticket, CancellationToken cancellationToken = default)
    {
        if (DateTimeOffset.FromUnixTimeSeconds(ticket.Expires) <= DateTimeOffset.UtcNow)
        {
            throw new LibreLinkAuthenticationExpiredException();
        }

        _authTicket = ticket;

        return Task.FromResult(new LoginResponse
        {
            AuthTicket = ticket
        });
    }
}