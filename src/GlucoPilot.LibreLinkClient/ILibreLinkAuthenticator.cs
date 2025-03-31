using GlucoPilot.LibreLinkClient.Exceptions;
using GlucoPilot.LibreLinkClient.Models;
using System.Threading;
using System.Threading.Tasks;

namespace GlucoPilot.LibreLinkClient;

internal interface ILibreLinkAuthenticator
{
    /// <summary>
    /// A user is not authenticated when the auth ticket is null or expired.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Log in to LibreLink.
    /// </summary>
    /// <param name="username">User name.</param>
    /// <param name="password">Password.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    Task<LoginResponse> LoginAsync(string username, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticate the client with an existing auth ticket.
    /// </summary>
    /// <param name="ticket">The existing auth ticket.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="LibreLinkAuthenticationExpiredException">
    /// When the auth ticket is expired.
    /// </exception>
    Task<LoginResponse> LoginAsync(AuthTicket ticket, CancellationToken cancellationToken = default);
}