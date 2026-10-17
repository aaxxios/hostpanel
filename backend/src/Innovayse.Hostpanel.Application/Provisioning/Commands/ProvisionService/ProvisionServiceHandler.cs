using System.Security.Cryptography;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Provisioning;
using Innovayse.Hostpanel.Domain.Servers;
using Innovayse.Hostpanel.Domain.Servers.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace Innovayse.Hostpanel.Application.Provisioning.Commands.ProvisionService;

/// <summary>
/// Provisions a pending hosting service by selecting the best server, calling the appropriate
/// provisioning provider, activating the service, and persisting all changes.
/// </summary>
public sealed class ProvisionServiceHandler
{
    /// <summary>Length, in bytes, of the random password generated for a new hosting account.</summary>
    private const int GeneratedPasswordByteLength = 12;

    /// <summary>Length, in characters, the generated password is truncated to.</summary>
    private const int GeneratedPasswordLength = 16;

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Creates the provisioning provider configured for the selected server.</summary>
    private readonly IProvisioningProviderFactory _providerFactory;

    /// <summary>Selects the optimal server using the proportional-fill strategy.</summary>
    private readonly IServerSelector _serverSelector;

    /// <summary>Logs a provider failure without putting its detail on the wire.</summary>
    private readonly ILogger<ProvisionServiceHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="providerFactory">Creates the provisioning provider configured for the selected server.</param>
    /// <param name="serverSelector">Selects the optimal server using the proportional-fill strategy.</param>
    /// <param name="logger">Logs a provider failure without putting its detail on the wire.</param>
    public ProvisionServiceHandler(
        IHostpanelDbContext context,
        IProvisioningProviderFactory providerFactory,
        IServerSelector serverSelector,
        ILogger<ProvisionServiceHandler> logger)
    {
        _context = context;
        _providerFactory = providerFactory;
        _serverSelector = serverSelector;
        _logger = logger;
    }

    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="cmd">The service to provision.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>SERVICE_NOT_FOUND</c> when the id names no service,
    /// <c>NO_ELIGIBLE_SERVER</c> when no server of the product's module has room, or
    /// <c>PROVISIONING_UPSTREAM_FAILED</c> when the provider rejects the request.
    /// </returns>
    public async Task<Result> HandleAsync(ProvisionServiceCommand cmd, CancellationToken ct)
    {
        var service = await _context.ClientServices.FirstOrDefaultAsync(s => s.Id == cmd.ServiceId, ct);
        if (service is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.SERVICE_NOT_FOUND);
        }

        // Look up the product to get the hosting package name and server group.
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == service.ProductId, ct);

        // Select the best server — prefer the product's server group, fall back to module-level selection.
        var server = product?.ServerGroupId is { } sgId
            ? await _serverSelector.SelectFromGroupAsync(sgId, ct)
            : await _serverSelector.SelectAsync(ServerModule.Cwp7, ct);
        if (server is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.NO_ELIGIBLE_SERVER);
        }

        var request = new ProvisionAccountOrder(
            service.Id,
            service.Domain ?? $"temp{service.Id}.example.com",
            service.Username ?? $"user{service.Id}",
            GeneratePassword(),
            product?.PackageName ?? "default");

        ProvisioningResult result;
        try
        {
            var provider = _providerFactory.CreateFor(server);
            result = await provider.ProvisionAsync(request, ct);
        }
        catch (Exception ex)
        {
            // The provider's own message and the server it named never reach the caller (rules
            // on upstream detail); they are logged here for whoever has to chase the failure up.
            _logger.LogWarning(ex, "Provisioning service {ServiceId} on server {ServerId} failed.", service.Id, server.Id);
            return Result.FailureCode(HostpanelErrorCodes.PROVISIONING_UPSTREAM_FAILED);
        }

        if (!result.Success)
        {
            _logger.LogWarning(
                "Provisioning service {ServiceId} on server {ServerId} was refused: {ErrorMessage}",
                service.Id, server.Id, result.ErrorMessage);
            return Result.FailureCode(HostpanelErrorCodes.PROVISIONING_UPSTREAM_FAILED);
        }

        // Record the server alongside the activation: the suspend, unsuspend, terminate and
        // single-sign-on handlers all give up when ServerId is null, so a service activated
        // without one could never be acted on again.
        service.ActivateOn(server.Id, result.ProvisioningRef!);

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }

    /// <summary>Generates a cryptographically random password for the new hosting account.</summary>
    /// <returns>A 16-character alphanumeric password string.</returns>
    private static string GeneratePassword()
    {
        var bytes = RandomNumberGenerator.GetBytes(GeneratedPasswordByteLength);
        return Convert.ToBase64String(bytes).Replace("=", string.Empty, StringComparison.Ordinal)[..GeneratedPasswordLength];
    }
}
