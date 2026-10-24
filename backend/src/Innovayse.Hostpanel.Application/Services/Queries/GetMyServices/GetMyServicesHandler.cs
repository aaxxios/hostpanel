using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Services.Queries.GetMyServices;

/// <summary>Returns the calling client's services as DTOs with product names.</summary>
public sealed class GetMyServicesHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Who is asking; the query does not say, and must not.</summary>
    private readonly ICurrentUserService _caller;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="caller">Who is asking; the query does not say, and must not.</param>
    public GetMyServicesHandler(IHostpanelDbContext context, ICurrentUserService caller)
    {
        _context = context;
        _caller = caller;
    }

    /// <summary>
    /// Handles the query.
    /// </summary>
    /// <param name="qry">The query. It names no account: this reads the caller's own.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The caller's own services, or <c>CLIENT_PROFILE_NOT_FOUND</c> when no client record exists
    /// for the caller.
    /// </returns>
    public async Task<Result<IReadOnlyList<ClientServiceDto>>> HandleAsync(
        GetMyServicesQuery qry, CancellationToken ct)
    {
        var userId = _caller.RequireUserId();

        // Was IClientRepository.FindByUserIdAsync -- read-only, so no tracking is needed.
        var client = await _context.Clients.FindReadOnlyByUserIdAsync(userId, ct);
        if (client is null)
        {
            return Result<IReadOnlyList<ClientServiceDto>>.FailureCode(HostpanelErrorCodes.CLIENT_PROFILE_NOT_FOUND);
        }

        // Was IClientServiceRepository.ListByClientAsync -- read-only, so no tracking is needed.
        var services = await _context.ClientServices
            .AsNoTracking()
            .Where(s => s.ClientId == client.Id)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);

        // Was IProductRepository.FindByIdsAsync -- read-only, so no tracking is needed.
        var productIds = services.Select(s => s.ProductId).Distinct().ToList();
        var products = await _context.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(ct);
        var productMap = products.ToDictionary(p => p.Id, p => p.Name);

        // Was IServerRepository.FindByIdAsync, called once per distinct server id -- batched here
        // into one query with the same result. Read-only, so no tracking is needed.
        var serverIds = services.Where(s => s.ServerId.HasValue).Select(s => s.ServerId!.Value).Distinct().ToList();
        var servers = await _context.Servers
            .AsNoTracking()
            .Where(s => serverIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => (s.Name, s.Hostname, s.IpAddress), ct);

        var dtos = services
            .Select(svc =>
            {
                string? serverName = null;
                string? serverHostname = null;
                string? serverIp = null;
                if (svc.ServerId.HasValue && servers.TryGetValue(svc.ServerId.Value, out var srv))
                {
                    serverName = srv.Name;
                    serverHostname = srv.Hostname;
                    serverIp = srv.IpAddress;
                }

                return new ClientServiceDto(
                    svc.Id,
                    svc.ProductId,
                    productMap.GetValueOrDefault(svc.ProductId),
                    svc.BillingCycle,
                    svc.Status,
                    svc.NextRenewalAt,
                    svc.Domain,
                    svc.Username,
                    svc.RecurringAmount,
                    svc.FirstPaymentAmount,
                    svc.PaymentMethod,
                    svc.ServerId,
                    serverName,
                    serverHostname,
                    serverIp,
                    svc.CreatedAt);
            })
            .ToList();

        return Result<IReadOnlyList<ClientServiceDto>>.Success(dtos);
    }
}
