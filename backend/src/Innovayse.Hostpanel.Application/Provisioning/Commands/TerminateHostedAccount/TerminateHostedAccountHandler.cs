using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Provisioning.Common;
using Innovayse.Hostpanel.Application.Provisioning.Extensions;
using Innovayse.Hostpanel.Domain.Provisioning;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Innovayse.Hostpanel.Application.Provisioning.Commands.TerminateHostedAccount;

/// <summary>
/// Permanently terminates a hosting service by looking up the assigned server, calling the
/// provisioning provider, and marking the service as terminated.
/// </summary>
public sealed class TerminateHostedAccountHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Creates the provisioning provider configured for the service's server.</summary>
    private readonly IProvisioningProviderFactory _providerFactory;

    /// <summary>Logs a provider failure without putting its detail on the wire.</summary>
    private readonly ILogger<TerminateHostedAccountHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="providerFactory">Creates the provisioning provider configured for the service's server.</param>
    /// <param name="logger">Logs a provider failure without putting its detail on the wire.</param>
    public TerminateHostedAccountHandler(
        IHostpanelDbContext context,
        IProvisioningProviderFactory providerFactory,
        ILogger<TerminateHostedAccountHandler> logger)
    {
        _context = context;
        _providerFactory = providerFactory;
        _logger = logger;
    }

    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="cmd">The service to terminate and the reason.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>SERVICE_NOT_FOUND</c> when the id names no service,
    /// <c>SERVICE_NOT_PROVISIONED</c> when the service has never been provisioned, or
    /// <c>SERVICE_TERMINATE_UPSTREAM_FAILED</c> when the provider rejects the request. A service
    /// with no usable server is still marked terminated on the platform -- there is nowhere
    /// upstream to tell, and the platform's own record must not be blocked by that.
    /// </returns>
    public async Task<Result> HandleAsync(TerminateHostedAccountCommand cmd, CancellationToken ct)
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
                    var request = new TerminateAccountOrder(service.Id, service.ProvisioningRef, cmd.Reason);
                    await provider.TerminateAsync(request, ct);
                }
                catch (Exception ex)
                {
                    // The provider's own message and the server it named never reach the caller
                    // (rules on upstream detail); they are logged here for whoever has to chase
                    // the failure up.
                    _logger.LogWarning(
                        ex, "Terminating service {ServiceId} on server {ServerId} failed.", service.Id, server.Id);
                    return Result.FailureCode(HostpanelErrorCodes.SERVICE_TERMINATE_UPSTREAM_FAILED);
                }
            }
        }

        // The provider was asked above and its failure answered, so the event that would ask again is skipped.
        service.Terminate(cmd.Reason, hostingAlreadyUpdated: true);

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
