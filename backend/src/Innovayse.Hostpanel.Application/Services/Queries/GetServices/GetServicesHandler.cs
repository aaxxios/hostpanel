using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Shared.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Services.Queries.GetServices;
/// <summary>Returns a paginated list of all client services for admin use, enriched with client, domain, and pricing data.</summary>
public sealed class GetServicesHandler
{
    /// <summary>The stored cycle string that bills annually; anything else is read as monthly.</summary>
    private const string AnnualCycle = BillingCycleNames.Annual;

    /// <summary>Price shown when the product has no price in the client's currency for that cycle.</summary>
    private const decimal NoPrice = 0m;

    /// <summary>The data context the services, the owning clients and the linked domains are read from.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Resolves the currency each owning client is billed in.</summary>
    private readonly IPayerCurrencyResolver _payerCurrency;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The data context the services, the owning clients and the linked domains are read from.</param>
    /// <param name="payerCurrency">Resolves the currency each owning client is billed in.</param>
    public GetServicesHandler(IHostpanelDbContext context, IPayerCurrencyResolver payerCurrency)
    {
        _context = context;
        _payerCurrency = payerCurrency;
    }

    /// <summary>
    /// Handles the query by resolving product names, client names, linked domains, and the price
    /// in each owning client's currency for the billing cycle.
    /// </summary>
    /// <param name="qry">The page, page size and optional client filter.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, always: an out-of-range page answers an empty page, not a refusal.</returns>
    public async Task<Result<PagedResult<ServiceListItemDto>>> HandleAsync(GetServicesQuery qry, CancellationToken ct)
    {
        var servicesQuery = _context.ClientServices.AsQueryable();
        if (qry.ClientId.HasValue)
        {
            servicesQuery = servicesQuery.Where(s => s.ClientId == qry.ClientId.Value);
        }

        var total = await servicesQuery.CountAsync(ct);
        var services = await servicesQuery
            .OrderByDescending(s => s.CreatedAt)
            .Paginate(qry.Page, qry.PageSize)
            .ToListAsync(ct);

        // Batch-resolve products. Was IProductRepository.FindByIdsAsync, which always included
        // the per-currency prices -- preserved here since the price is read below through
        // PriceFor. Read-only, so no tracking is needed.
        var productIds = services.Select(s => s.ProductId).Distinct().ToList();
        var products = await _context.Products
            .AsNoTracking()
            .Include(p => p.Prices)
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(ct);
        var productMap = products.ToDictionary(p => p.Id);

        // Batch-resolve client names
        var clientIds = services.Select(s => s.ClientId).Distinct().ToList();
        var clients = await _context.Clients.Where(c => clientIds.Contains(c.Id)).ToListAsync(ct);
        var clientMap = clients.ToDictionary(c => c.Id, c => $"{c.FirstName} {c.LastName}");

        // Resolve each owning client's billing currency once; a service is priced in that.
        var currencyMap = new Dictionary<int, Currency>();
        foreach (var clientId in clientIds)
        {
            currencyMap[clientId] = await _payerCurrency.ForClientAsync(clientId, ct);
        }

        // Batch-resolve linked domain names; a service that runs on a site of its own, not on a registered domain, shows its own domain
        var serviceIds = services.Select(s => s.Id).ToList();
        var domainMap = await _context.Domains
            .Where(d => d.LinkedServiceId != null && serviceIds.Contains(d.LinkedServiceId.Value))
            .ToDictionaryAsync(d => d.LinkedServiceId!.Value, d => d.Name, ct);

        var result = services
            .Select(svc =>
            {
                var product = productMap.GetValueOrDefault(svc.ProductId);
                var currency = currencyMap[svc.ClientId];
                var cycle = svc.BillingCycle == AnnualCycle ? BillingCycle.Annual : BillingCycle.Monthly;
                var price = product?.PriceFor(currency.Code, cycle) ?? NoPrice;

                return new ServiceListItemDto(
                    svc.Id,
                    svc.ClientId,
                    clientMap.GetValueOrDefault(svc.ClientId),
                    product?.Name,
                    domainMap.GetValueOrDefault(svc.Id) ?? svc.Domain,
                    price,
                    currency.Code,
                    svc.BillingCycle,
                    svc.Status,
                    svc.NextRenewalAt);
            })
            .ToList();

        return Result<PagedResult<ServiceListItemDto>>.Success(
            new PagedResult<ServiceListItemDto>(result, total, qry.Page, qry.PageSize));
    }
}
