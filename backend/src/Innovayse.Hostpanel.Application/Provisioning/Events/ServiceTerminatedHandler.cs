using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Provisioning.Common;
using Innovayse.Hostpanel.Application.Provisioning.Extensions;
using Innovayse.Hostpanel.Domain.Provisioning;
using Innovayse.Hostpanel.Domain.Provisioning.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Innovayse.Hostpanel.Application.Provisioning.Events;

/// <summary>
/// Handles <see cref="ServiceTerminatedEvent"/> by terminating the hosting account on the
/// assigned server's provisioning provider. Delivered asynchronously.
/// </summary>
public sealed class ServiceTerminatedHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Creates the provisioning provider configured for the service's server.</summary>
    private readonly IProvisioningProviderFactory _providerFactory;

    /// <summary>Logger for provider failures that leave the server out of step with the platform.</summary>
    private readonly ILogger<ServiceTerminatedHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="providerFactory">Creates the provisioning provider configured for the service's server.</param>
    /// <param name="logger">Logger for provider failures that leave the server out of step with the platform.</param>
    public ServiceTerminatedHandler(
        IHostpanelDbContext context, IProvisioningProviderFactory providerFactory, ILogger<ServiceTerminatedHandler> logger)
    {
        _context = context;
        _providerFactory = providerFactory;
        _logger = logger;
    }

    /// <summary>
    /// Terminates the hosting account on the provider when a service is permanently terminated.
    /// </summary>
    /// <param name="evt">The domain event carrying the service identifier and reason.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task HandleAsync(ServiceTerminatedEvent evt, CancellationToken ct)
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
            var request = new TerminateAccountOrder(service.Id, service.ProvisioningRef, evt.Reason);
            await provider.TerminateAsync(request, ct);
        }
        catch (Exception ex)
        {
            // The benign case — the command handler already terminated the account — is not
            // distinguishable here from access denied, an unreachable host, or a module the
            // factory has no provider for. The handler continues, but no longer in silence: a
            // termination that never reached the server leaves a live, unbilled hosting account.
            _logger.LogWarning(
                ex,
                "Terminating service {ServiceId} on server {ServerId} failed; the platform shows it as terminated but the hosting account may still exist.",
                service.Id,
                server.Id);
        }
    }
}
