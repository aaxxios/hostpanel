using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Domain.Provisioning;
using Innovayse.Hostpanel.Domain.Servers;
using Innovayse.Hostpanel.Domain.Servers.Enums;
using Innovayse.Hostpanel.Domain.Services;
using Innovayse.Hostpanel.Domain.Services.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
namespace Innovayse.Hostpanel.Application.Services.Commands.SetupService;
/// <summary>
/// Handles <see cref="SetupServiceCommand"/> by selecting the best server,
/// provisioning the hosting account via the appropriate provider,
/// and activating the service.
/// </summary>
public sealed class SetupServiceHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Factory to create per-server provisioning providers.</summary>
    private readonly IProvisioningProviderFactory _providerFactory;

    /// <summary>Selects the optimal server using proportional fill strategy.</summary>
    private readonly IServerSelector _serverSelector;

    /// <summary>Unit of work for persisting changes.</summary>
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="providerFactory">Factory to create per-server provisioning providers.</param>
    /// <param name="serverSelector">Selects the optimal server using proportional fill strategy.</param>
    /// <param name="unitOfWork">Unit of work for persisting changes.</param>
    public SetupServiceHandler(
        IHostpanelDbContext context,
        IProvisioningProviderFactory providerFactory,
        IServerSelector serverSelector,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _providerFactory = providerFactory;
        _serverSelector = serverSelector;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Sets up the service with the provided domain, username, and password,
    /// selects the best available server, provisions the account, and activates the service.
    /// </summary>
    /// <param name="cmd">The setup command with hosting details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>SERVICE_NOT_FOUND</c> when the id names no service, <c>SERVICE_NOT_PENDING</c>
    /// when the service is not Pending, <c>NO_ELIGIBLE_SERVER</c> when no server has room, or
    /// <c>PROVISIONING_UPSTREAM_FAILED</c> when the provider rejects the request.
    /// </returns>
    public async Task<Result> HandleAsync(SetupServiceCommand cmd, CancellationToken ct)
    {
        // Was IClientServiceRepository.FindByIdAsync. Tracked, not AsNoTracking: the service is
        // mutated and saved further down.
        var service = await _context.ClientServices.FirstOrDefaultAsync(s => s.Id == cmd.ServiceId, ct);
        if (service is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.SERVICE_NOT_FOUND);
        }

        if (service.Status is not ServiceStatus.Pending)
        {
            return Result.FailureCode(HostpanelErrorCodes.SERVICE_NOT_PENDING);
        }

        // Determine the server module type from the product (default to hosting panel (v7) for now).
        // Was IProductRepository.FindByIdAsync -- read-only, so no tracking is needed. The
        // product's per-currency prices are not read here, so no Include is needed either.
        var module = ServerModule.Cwp7;
        var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == service.ProductId, ct);
        if (product is not null)
        {
            module = MapProductTypeToModule(product.Type);
        }

        // Select the best available server using proportional fill
        var server = await _serverSelector.SelectAsync(module, ct);
        if (server is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.NO_ELIGIBLE_SERVER);
        }

        // Create a provider for the selected server
        var provider = _providerFactory.CreateFor(server);

        // Provision the account
        var request = new ProvisionAccountOrder(
            service.Id,
            cmd.Domain,
            cmd.Username,
            cmd.Password,
            product?.PackageName ?? "default");

        var result = await provider.ProvisionAsync(request, ct);

        if (!result.Success)
        {
            return Result.FailureCode(HostpanelErrorCodes.PROVISIONING_UPSTREAM_FAILED);
        }

        // Save the client-provided hosting details and assign the server
        service.Update(
            domain: cmd.Domain,
            dedicatedIp: service.DedicatedIp,
            username: cmd.Username,
            password: cmd.Password,
            billingCycle: service.BillingCycle,
            recurringAmount: service.RecurringAmount,
            paymentMethod: service.PaymentMethod,
            nextRenewalAt: service.NextRenewalAt,
            subscriptionId: service.SubscriptionId,
            overrideAutoSuspend: service.OverrideAutoSuspend,
            suspendUntil: service.SuspendUntil,
            autoTerminateEndOfCycle: service.AutoTerminateEndOfCycle,
            autoTerminateReason: service.AutoTerminateReason,
            adminNotes: service.AdminNotes,
            provisioningRef: service.ProvisioningRef,
            firstPaymentAmount: service.FirstPaymentAmount,
            promotionCode: service.PromotionCode,
            terminatedAt: service.TerminatedAt,
            serverId: server.Id,
            quantity: service.Quantity,
            productId: null);

        service.Activate(result.ProvisioningRef!);

        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    /// <summary>
    /// Maps a product type to the server module that provisions it.
    /// Currently all hosting products use hosting panel (v7).
    /// </summary>
    /// <param name="type">The product type.</param>
    /// <returns>The corresponding server module.</returns>
    private static ServerModule MapProductTypeToModule(ProductType type) => type switch
    {
        ProductType.SharedHosting => ServerModule.Cwp7,
        ProductType.Vps => ServerModule.Cwp7,
        ProductType.Dedicated => ServerModule.Cwp7,
        _ => ServerModule.Cwp7,
    };
}
