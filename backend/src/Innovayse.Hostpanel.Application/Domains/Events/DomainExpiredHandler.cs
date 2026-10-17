using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Application.Domains.Extensions;
using Innovayse.Hostpanel.Application.Notifications.Commands.SendEmail;
using Innovayse.Hostpanel.Application.Services.Commands.SuspendService;
using Innovayse.Hostpanel.Domain.Domains.Events;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Domains.Events;

/// <summary>
/// Handles <see cref="DomainExpiredEvent"/> raised when a domain transitions to the Expired status.
/// Dispatches a <see cref="SuspendServiceCommand"/> for the linked hosting service, if any,
/// and sends a domain-expired notification email to the owning client.
/// </summary>
public sealed class DomainExpiredHandler
{
    /// <summary>Wolverine bus used to suspend the linked service and to send the notification email.</summary>
    private readonly IMessageBus _bus;

    /// <summary>The data context the domain and the owning client are read from.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Resolves the owning client's identity user, for the notification email address.</summary>
    private readonly IIdentityProvider _identity;

    /// <summary>Structured logger.</summary>
    private readonly ILogger<DomainExpiredHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="bus">Wolverine bus used to suspend the linked service and to send the notification email.</param>
    /// <param name="context">The data context the domain and the owning client are read from.</param>
    /// <param name="identity">Resolves the owning client's identity user, for the notification email address.</param>
    /// <param name="logger">Structured logger.</param>
    public DomainExpiredHandler(
        IMessageBus bus, IHostpanelDbContext context, IIdentityProvider identity, ILogger<DomainExpiredHandler> logger)
    {
        _bus = bus;
        _context = context;
        _identity = identity;
        _logger = logger;
    }

    /// <summary>
    /// Handles the domain expired event by suspending the linked hosting service
    /// and sending a notification email.
    /// </summary>
    /// <param name="evt">The domain expired event.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task HandleAsync(DomainExpiredEvent evt, CancellationToken ct)
    {
        _logger.LogInformation(
            "Domain {DomainId} (client {ClientId}) has expired.",
            evt.DomainId,
            evt.ClientId);

        if (evt.LinkedServiceId.HasValue)
        {
            try
            {
                var outcome = await _bus.InvokeAsync<Result>(
                    new SuspendServiceCommand(evt.LinkedServiceId.Value), ct);

                if (outcome.IsSuccess)
                {
                    _logger.LogInformation(
                        "Suspended linked service {ServiceId} due to domain {DomainId} expiry.",
                        evt.LinkedServiceId.Value,
                        evt.DomainId);
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to suspend linked service {ServiceId} for expired domain {DomainId} ({Code}). The service may need manual suspension.",
                        evt.LinkedServiceId.Value,
                        evt.DomainId,
                        outcome.ErrorCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to suspend linked service {ServiceId} for expired domain {DomainId}. The service may need manual suspension.",
                    evt.LinkedServiceId.Value,
                    evt.DomainId);
            }
        }

        await SendNotificationAsync(evt, ct);
    }

    /// <summary>
    /// Sends a domain-expired notification email to the owning client.
    /// Failures are logged as warnings and do not propagate.
    /// </summary>
    /// <param name="evt">The domain expired event.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous send operation.</returns>
    private async Task SendNotificationAsync(DomainExpiredEvent evt, CancellationToken ct)
    {
        try
        {
            var client = await _context.Clients
                .IncludeChildren()
                .FirstOrDefaultAsync(c => c.Id == evt.ClientId, ct);
            if (client is null)
            {
                return;
            }

            var user = await _identity.FindBySubjectAsync(client.UserId, ct);
            if (user is null)
            {
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
                    name = domain?.Name,
                    linkedServiceId = evt.LinkedServiceId,
                }
            };

            await _bus.InvokeAsync(new SendEmailCommand(user.Email, "domain-expired", data), ct);

            _logger.LogInformation(
                "Sent domain-expired notification for domain {DomainId} to {Email}.",
                evt.DomainId,
                user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to send domain-expired notification for domain {DomainId}.",
                evt.DomainId);
        }
    }
}
