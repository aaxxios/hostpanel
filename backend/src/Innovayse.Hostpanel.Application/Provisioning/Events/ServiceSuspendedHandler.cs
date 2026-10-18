using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Provisioning.Common;
using Innovayse.Hostpanel.Application.Provisioning.Extensions;
using Innovayse.Hostpanel.Domain.Provisioning;
using Innovayse.Hostpanel.Domain.Provisioning.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Innovayse.Hostpanel.Application.Provisioning.Events;

/// <summary>
/// Handles <see cref="ServiceSuspendedEvent"/> by suspending the hosting account on the assigned
/// server's provisioning provider. Delivered asynchronously.
/// </summary>
public sealed class ServiceSuspendedHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Creates the provisioning provider configured for the service's server.</summary>
    private readonly IProvisioningProviderFactory _providerFactory;

    /// <summary>Logger for provider failures that leave the server out of step with the platform.</summary>
    private readonly ILogger<ServiceSuspendedHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="providerFactory">Creates the provisioning provider configured for the service's server.</param>
    /// <param name="logger">Logger for provider failures that leave the server out of step with the platform.</param>
    public ServiceSuspendedHandler(
        IHostpanelDbContext context, IProvisioningProviderFactory providerFactory, ILogger<ServiceSuspendedHandler> logger)
    {
        _context = context;
        _providerFactory = providerFactory;
        _logger = logger;
    }

    /// <summary>
    /// Suspends the hosting account on the provider when a service is suspended.
    /// </summary>
    /// <param name="evt">The domain event carrying the service identifier and reason.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task HandleAsync(ServiceSuspendedEvent evt, CancellationToken ct)
    {
        var service = await _context.ClientServices.FirstOrDefaultAsync(s => s.Id == evt.ServiceId, ct);
        if (service?.ProvisioningRef is null || service.ServerId is null)
        {
            return;
        }

        var server = await _context.FindServerAsync(service, ct);
        if (server is null)
        {
            return;
        }

        try
        {
            var provider = _providerFactory.CreateFor(server);
            var request = new SuspendAccountOrder(service.Id, service.ProvisioningRef, evt.Reason);
            await provider.SuspendAsync(request, ct);
        }
        catch (Exception ex)
        {
            // The benign case — the command handler already suspended the account on the server —
            // is not distinguishable here: the provider reports it the same way it reports access
            // denied, an unreachable host, or a module the factory has no provider for. So the
            // handler still continues, but it no longer does so silently. Without this line a
            // suspension that never reached the server leaves the platform showing "Suspended"
            // while the site stays up and billing stops, with nothing anywhere to say so.
            _logger.LogWarning(
                ex,
                "Suspending service {ServiceId} on server {ServerId} failed; the platform shows it as suspended but the hosting account may still be live.",
                service.Id,
                server.Id);
        }
    }
}
