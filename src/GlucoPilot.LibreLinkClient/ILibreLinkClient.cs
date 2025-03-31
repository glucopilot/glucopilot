using GlucoPilot.LibreLinkClient.Exceptions;
using GlucoPilot.LibreLinkClient.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GlucoPilot.LibreLinkClient;

public interface ILibreLinkClient
{
    /// <summary>
    /// Get LibreLink connections.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>LibreLink connections.</returns>
    Task<IReadOnlyCollection<ConnectionData>> GetConnectionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Log in to LibreLink.
    /// </summary>
    /// <param name="username">User name.</param>
    /// <param name="password">Password.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="LibreLinkAuthenticationFailedException">
    /// When authentication fails.
    /// </exception>
    /// <returns>The successful auth ticket.</returns>
    Task<AuthTicket> LoginAsync(string username, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticate the client with an existing auth ticket.
    /// </summary>
    /// <param name="ticket">The existing auth ticket.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="LibreLinkAuthenticationExpiredException">
    /// When the auth ticket is expired.
    /// </exception>
    Task LoginAsync(AuthTicket ticket, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get LibreLink graph information. This contains graph data and connection information.
    /// </summary>
    /// <param name="patientId">Patient Id to get graph information for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    Task<GraphInformation?> GraphAsync(Guid patientId, CancellationToken cancellationToken = default);
}