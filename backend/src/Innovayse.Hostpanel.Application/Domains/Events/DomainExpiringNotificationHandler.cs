using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Notifications.Commands.SendEmail;
using Innovayse.Hostpanel.Domain.Domains.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Domains.Events;

/// <summary>
/// Handles <see cref="DomainExpiringEvent"/> and dispatches a domain-expiring notification email
/// to the owning client.
/// </summary>
public sealed class DomainExpiringNotificationHandler
{
    /// <summary>Message bus, for dispatching the notification email command.</summary>
    private readonly IMessageBus _bus;

    /// <summary>The module's data context, for the client that owns the domain.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Reads people from wherever they live.</summary>
    private readonly IIdentityProvider _identity;

    /// <summary>Structured logger.</summary>
    private readonly ILogger<DomainExpiringNotificationHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="bus">Message bus, for dispatching the notification email command.</param>
    /// <param name="context">The module's data context, for the client that owns the domain.</param>
    /// <param name="identity">Reads people from wherever they live.</param>
    /// <param name="logger">Structured logger.</param>
    public DomainExpiringNotificationHandler(
        IMessageBus bus,
        IHostpanelDbContext context,
        IIdentityProvider identity,
        ILogger<DomainExpiringNotificationHandler> logger)
    {
        _bus = bus;
        _context = context;
        _identity = identity;
        _logger = logger;
    }

    /// <summary>
    /// Resolves the client email and sends a domain-expiring notification email.
    /// </summary>
    /// <param name="evt">The domain expiring domain event.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous send operation.</returns>
    public async Task HandleAsync(DomainExpiringEvent evt, CancellationToken ct)
    {
        try
        {
            // Was IClientRepository.FindByIdAsync -- read-only, so no tracking is needed. Contacts
            // and Users are not read here, so unlike the repository's Include-laden query, this one
            // stays a plain lookup: nothing below reads either navigation.
            var client = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == evt.ClientId, ct);
            if (client is null)
            {
                _logger.LogWarning(
                    "Client {ClientId} not found for domain expiring notification (domain {DomainId}).",
                    evt.ClientId,
                    evt.DomainId);
                return;
            }

            var user = await _identity.FindBySubjectAsync(client.UserId, ct);
            if (user is null)
            {
                _logger.LogWarning(
                    "User {UserId} not found for domain expiring notification (domain {DomainId}).",
                    client.UserId,
                    evt.DomainId);
                return;
            }

            var daysUntilExpiry = (int)Math.Ceiling((evt.ExpiresAt - DateTimeOffset.UtcNow).TotalDays);

            var data = new
            {
                domain = new
                {
                    id = evt.DomainId,
                    name = evt.Name,
                    expiresAt = evt.ExpiresAt.ToString("yyyy-MM-dd"),
                    daysUntilExpiry,
                }
            };

            await _bus.InvokeAsync(new SendEmailCommand(user.Email, "domain-expiring", data), ct);

            _logger.LogInformation(
                "Sent domain-expiring notification for domain {DomainName} (ID {DomainId}, expires in {Days} days) to {Email}.",
                evt.Name,
                evt.DomainId,
                daysUntilExpiry,
                user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to send domain-expiring notification for domain {DomainId}.",
                evt.DomainId);
        }
    }
}
