using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Application.Domains.Extensions;
using Innovayse.Hostpanel.Application.Notifications.Commands.SendEmail;
using Innovayse.Hostpanel.Application.Services.Commands.UnsuspendService;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Domain.Domains.Events;
using Innovayse.Hostpanel.Domain.Services;
using Innovayse.Hostpanel.Domain.Services.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wolverine;
namespace Innovayse.Hostpanel.Application.Domains.Events;

/// <summary>
/// Handles <see cref="DomainRenewedEvent"/> raised when a domain is successfully renewed.
/// If the domain has a linked hosting service that is currently <see cref="ServiceStatus.Suspended"/>,
/// dispatches an <see cref="UnsuspendServiceCommand"/> to reactivate it.
/// Also sends a domain-renewed notification email to the owning client.
/// </summary>
public sealed class DomainRenewedHandler
{
    /// <summary>The data context the domain, the linked service and the owning client are read from.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Resolves the owning client's identity user, for the notification email address.</summary>
    private readonly IIdentityProvider _identity;

    /// <summary>Wolverine bus used to unsuspend the linked service and to send the notification email.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Structured logger.</summary>
    private readonly ILogger<DomainRenewedHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The data context the domain, the linked service and the owning client are read from.</param>
    /// <param name="identity">Resolves the owning client's identity user, for the notification email address.</param>
    /// <param name="bus">Wolverine bus used to unsuspend the linked service and to send the notification email.</param>
    /// <param name="logger">Structured logger.</param>
    public DomainRenewedHandler(
        IHostpanelDbContext context, IIdentityProvider identity, IMessageBus bus, ILogger<DomainRenewedHandler> logger)
    {
        _context = context;
        _identity = identity;
        _bus = bus;
        _logger = logger;
    }

    /// <summary>
    /// Handles the domain renewed event by unsuspending the linked hosting service when applicable.
    /// </summary>
    /// <param name="evt">The domain renewed event.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task HandleAsync(DomainRenewedEvent evt, CancellationToken ct)
    {
        _logger.LogInformation(
            "Domain {DomainId} (client {ClientId}) has been renewed until {NewExpiresAt}.",
            evt.DomainId,
            evt.ClientId,
            evt.NewExpiresAt);

        var domain = await _context.Domains
            .IncludeChildren()
            .FirstOrDefaultAsync(x => x.Id == evt.DomainId, ct);

        if (domain?.LinkedServiceId is { } linkedServiceId)
        {
            await TryUnsuspendLinkedServiceAsync(evt.DomainId, linkedServiceId, ct);
        }

        await SendNotificationAsync(evt, domain, ct);
    }

    /// <summary>
    /// Attempts to unsuspend a linked hosting service after the domain is renewed.
    /// Only unsuspends if the service is currently in <see cref="ServiceStatus.Suspended"/> status.
    /// </summary>
    /// <param name="domainId">The renewed domain ID.</param>
    /// <param name="linkedServiceId">The linked hosting service ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous unsuspend operation.</returns>
    private async Task TryUnsuspendLinkedServiceAsync(int domainId, int linkedServiceId, CancellationToken ct)
    {
        try
        {
            var service = await _context.ClientServices.FirstOrDefaultAsync(s => s.Id == linkedServiceId, ct);

            if (service is null)
            {
                _logger.LogWarning(
                    "Linked service {ServiceId} for domain {DomainId} not found. Skipping unsuspend.",
                    linkedServiceId,
                    domainId);
                return;
            }

            if (service.Status != ServiceStatus.Suspended)
            {
                _logger.LogInformation(
                    "Linked service {ServiceId} for domain {DomainId} is in status {Status}, not Suspended. Skipping unsuspend.",
                    linkedServiceId,
                    domainId,
                    service.Status);
                return;
            }

            var outcome = await _bus.InvokeAsync<Result>(new UnsuspendServiceCommand(linkedServiceId), ct);

            if (outcome.IsSuccess)
            {
                _logger.LogInformation(
                    "Unsuspended linked service {ServiceId} after domain {DomainId} renewal.",
                    linkedServiceId,
                    domainId);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to unsuspend linked service {ServiceId} for renewed domain {DomainId} ({Code}). The service may need manual reactivation.",
                    linkedServiceId,
                    domainId,
                    outcome.ErrorCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to unsuspend linked service {ServiceId} for renewed domain {DomainId}. The service may need manual reactivation.",
                linkedServiceId,
                domainId);
        }
    }

    /// <summary>
    /// Sends a domain-renewed notification email to the owning client.
    /// Failures are logged as warnings and do not propagate.
    /// </summary>
    /// <param name="evt">The domain renewed event.</param>
    /// <param name="domain">The domain entity, or <see langword="null"/> if not found.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous send operation.</returns>
    private async Task SendNotificationAsync(DomainRenewedEvent evt, RegisteredDomain? domain, CancellationToken ct)
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

            var data = new
            {
                domain = new
                {
                    id = evt.DomainId,
                    name = domain?.Name,
                    newExpiresAt = evt.NewExpiresAt.ToString("yyyy-MM-dd"),
                }
            };

            await _bus.InvokeAsync(new SendEmailCommand(user.Email, "domain-renewed", data), ct);

            _logger.LogInformation(
                "Sent domain-renewed notification for domain {DomainId} to {Email}.",
                evt.DomainId,
                user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to send domain-renewed notification for domain {DomainId}.",
                evt.DomainId);
        }
    }
}
