using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Services.Queries.GetService;

/// <summary>Returns a single enriched client service for admin detail views.</summary>
public sealed class GetServiceHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetServiceHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles the query by loading the service and resolving the product name, client name, and
    /// server name.
    /// </summary>
    /// <param name="qry">The service to read.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The enriched detail, or <c>SERVICE_NOT_FOUND</c> when the id names no service.</returns>
    public async Task<Result<ServiceDetailDto>> HandleAsync(GetServiceQuery qry, CancellationToken ct)
    {
        // Was IClientServiceRepository.FindByIdAsync -- read-only, so no tracking is needed.
        var svc = await _context.ClientServices.AsNoTracking().FirstOrDefaultAsync(s => s.Id == qry.ServiceId, ct);
        if (svc is null)
        {
            return Result<ServiceDetailDto>.FailureCode(HostpanelErrorCodes.SERVICE_NOT_FOUND);
        }

        // Was IProductRepository.FindByIdAsync -- read-only, so no tracking is needed.
        var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == svc.ProductId, ct);

        // Was IClientRepository.FindByIdAsync -- read-only, so no tracking is needed.
        var client = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == svc.ClientId, ct);

        var clientName = client is not null
            ? $"{client.FirstName} {client.LastName}"
            : null;

        string? serverName = null;
        string? serverHostname = null;
        string? serverIp = null;
        if (svc.ServerId.HasValue)
        {
            // Was IServerRepository.FindByIdAsync -- read-only, so no tracking is needed.
            var server = await _context.Servers.AsNoTracking().FirstOrDefaultAsync(s => s.Id == svc.ServerId.Value, ct);
            serverName = server?.Name;
            serverHostname = server?.Hostname;
            serverIp = server?.IpAddress;
        }

        var dto = new ServiceDetailDto(
            svc.Id,
            svc.ClientId,
            clientName,
            svc.ProductId,
            product?.Name,
            svc.Domain,
            svc.DedicatedIp,
            svc.Username,
            svc.Password,
            svc.Quantity,
            svc.FirstPaymentAmount,
            svc.RecurringAmount,
            svc.PaymentMethod,
            svc.PromotionCode,
            svc.SubscriptionId,
            svc.BillingCycle,
            svc.Status.ToString(),
            svc.ProvisioningRef,
            svc.NextRenewalAt,
            svc.CreatedAt,
            svc.TerminatedAt,
            svc.OverrideAutoSuspend,
            svc.SuspendUntil,
            svc.AutoTerminateEndOfCycle,
            svc.AutoTerminateReason,
            svc.AdminNotes,
            svc.ServerId,
            serverName,
            serverHostname,
            serverIp);

        return Result<ServiceDetailDto>.Success(dto);
    }
}
