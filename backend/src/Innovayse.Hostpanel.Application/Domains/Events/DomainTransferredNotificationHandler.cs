using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Application.Domains.Extensions;
using Innovayse.Hostpanel.Application.Notifications.Commands.SendEmail;
using Innovayse.Hostpanel.Domain.Domains.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Domains.Events;

/// <summary>
/// Handles <see cref="DomainTransferredInEvent"/> and dispatches a domain-transferred notification email
/// to the owning client.
/// </summary>
public sealed class DomainTransferredNotificationHandler
{
    /// <summary>Wolverine bus used to send the notification email.</summary>
    private readonly IMessageBus _bus;

    /// <summary>The data context the domain and the owning client are read from.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Resolves the owning client's identity user, for the notification email address.</summary>
    private readonly IIdentityProvider _identity;

    /// <summary>Structured logger.</summary>
    private readonly ILogger<DomainTransferredNotificationHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="bus">Wolverine bus used to send the notification email.</param>
    /// <param name="context">The data context the domain and the owning client are read from.</param>
    /// <param name="identity">Resolves the owning client's identity user, for the notification email address.</param>
    /// <param name="logger">Structured logger.</param>
    public DomainTransferredNotificationHandler(
        IMessageBus bus,
        IHostpanelDbContext context,
        IIdentityProvider identity,
        ILogger<DomainTransferredNotificationHandler> logger)
    {
        _bus = bus;
        _context = context;
        _identity = identity;
        _logger = logger;
    }

    /// <summary>
    /// Resolves the client email and sends a domain-transferred notification email.
    /// </summary>
    /// <param name="evt">The domain transferred-in domain event.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous send operation.</returns>
    public async Task HandleAsync(DomainTransferredInEvent evt, CancellationToken ct)
    {
        try
        {
            var client = await _context.Clients
                .IncludeChildren()
                .FirstOrDefaultAsync(c => c.Id == evt.ClientId, ct);
            if (client is null)
            {
                _logger.LogWarning(
                    "Client {ClientId} not found for domain transferred notification (domain {DomainId}).",
                    evt.ClientId,
                    evt.DomainId);
                return;
            }

            var user = await _identity.FindBySubjectAsync(client.UserId, ct);
            if (user is null)
            {
                _logger.LogWarning(
                    "User {UserId} not found for domain transferred notification (domain {DomainId}).",
                    client.UserId,
                    evt.DomainId);
                return;
            }

            var domain = await _context.Domains
                .IncludeChildren()
                .FirstOrDefaultAsync(x => x.Id == evt.DomainId, ct);

            var data = new
            {
                domain = new
                {
                    id = evt.DomainId,
                    name = evt.Name,
                    registrar = domain?.Registrar,
                    expiresAt = domain?.ExpiresAt.ToString("yyyy-MM-dd"),
                }
            };

            await _bus.InvokeAsync(new SendEmailCommand(user.Email, "domain-transferred", data), ct);

            _logger.LogInformation(
                "Sent domain-transferred notification for domain {DomainName} (ID {DomainId}) to {Email}.",
                evt.Name,
                evt.DomainId,
                user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to send domain-transferred notification for domain {DomainId}.",
                evt.DomainId);
        }
    }
}
