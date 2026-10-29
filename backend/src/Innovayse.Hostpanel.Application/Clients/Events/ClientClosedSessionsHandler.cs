using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Clients.Events;
using Microsoft.Extensions.Logging;

namespace Innovayse.Hostpanel.Application.Clients.Events;

/// <summary>
/// Ends every session the closed account's owner holds, through each registered
/// <see cref="ISessionRevoker"/> (a security stamp change where this deployment owns the accounts),
/// so a session opened before the closing stops working at once. A non-notification handler, and
/// safe as one: revoking a subject with no session is not an error, and a failure is logged.
/// </summary>
public sealed class ClientClosedSessionsHandler
{
    /// <summary>
    /// Every place that holds sessions for the person: the panel's own sessions, and the client
    /// portal's when it keeps them in a shared store.
    /// </summary>
    private readonly IReadOnlyList<ISessionRevoker> _sessions;

    /// <summary>Records a revoker that failed, so the sessions can be ended by hand.</summary>
    private readonly ILogger<ClientClosedSessionsHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="sessions">Every registered revoker; each ends the sessions it knows about.</param>
    /// <param name="logger">Records a revoker that failed.</param>
    public ClientClosedSessionsHandler(IEnumerable<ISessionRevoker> sessions, ILogger<ClientClosedSessionsHandler> logger)
    {
        _sessions = sessions.ToList();
        _logger = logger;
    }

    /// <summary>Ends the closed account owner's sessions.</summary>
    /// <param name="evt">The closing.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task.</returns>
    public async Task HandleAsync(ClientClosedEvent evt, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(evt.UserId))
        {
            return;
        }

        try
        {
            foreach (var revoker in _sessions)
            {
                await revoker.RevokeAllAsync(evt.UserId, ct);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Wolverine combines every handler of one message into a single chain, so a throw here
            // would also stop the other handlers of the closing.
            _logger.LogError(ex, "Ending the sessions of closed client {ClientId} failed.", evt.ClientId);
        }
    }
}
