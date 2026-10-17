using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Provisioning.Common;
using Innovayse.Hostpanel.Application.Provisioning.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Innovayse.Hostpanel.Application.Provisioning.Commands.UnsuspendHostedAccount;

/// <summary>
/// Re-activates a suspended hosting service by looking up the assigned server, calling the
/// provisioning provider, and restoring the service to active status.
/// </summary>
public sealed class UnsuspendHostedAccountHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Creates the provisioning provider configured for the service's server.</summary>
    private readonly IProvisioningProviderFactory _providerFactory;

    /// <summary>Logs a provider failure without putting its detail on the wire.</summary>
    private readonly ILogger<UnsuspendHostedAccountHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="providerFactory">Creates the provisioning provider configured for the service's server.</param>
    /// <param name="logger">Logs a provider failure without putting its detail on the wire.</param>
    public UnsuspendHostedAccountHandler(
        IHostpanelDbContext context,
        IProvisioningProviderFactory providerFactory,
        ILogger<UnsuspendHostedAccountHandler> logger)
    {
        _context = context;
        _providerFactory = providerFactory;
        _logger = logger;
    }

    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="cmd">The service to reactivate.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>SERVICE_NOT_FOUND</c> when the id names no service,
    /// <c>SERVICE_NOT_PROVISIONED</c> when the service has never been provisioned, or
    /// <c>SERVICE_UNSUSPEND_UPSTREAM_FAILED</c> when the provider rejects the request. A service
    /// with no usable server is still marked active on the platform -- there is nowhere upstream
    /// to tell, and the platform's own record must not be blocked by that.
    /// </returns>
    public async Task<Result> HandleAsync(UnsuspendHostedAccountCommand cmd, CancellationToken ct)
    {
        var service = await _context.ClientServices.FirstOrDefaultAsync(s => s.Id == cmd.ServiceId, ct);
        if (service is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.SERVICE_NOT_FOUND);
        }

        if (service.ProvisioningRef is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.SERVICE_NOT_PROVISIONED);
        }

        if (service.ServerId is not null)
        {
            var server = await _context.FindServerAsync(service, ct);
            if (server is not null)
            {
                try
                {
                    var provider = _providerFactory.CreateFor(server);
                    await provider.UnsuspendAsync(service.ProvisioningRef, ct);
                }
                catch (Exception ex)
                {
                    // The provider's own message and the server it named never reach the caller
                    // (rules on upstream detail); they are logged here for whoever has to chase
                    // the failure up.
                    _logger.LogWarning(
                        ex, "Unsuspending service {ServiceId} on server {ServerId} failed.", service.Id, server.Id);
                    return Result.FailureCode(HostpanelErrorCodes.SERVICE_UNSUSPEND_UPSTREAM_FAILED);
                }
            }
        }

        // The provider was asked above and its failure answered, so the event that would ask again is skipped.
        service.Unsuspend(hostingAlreadyUpdated: true);

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
