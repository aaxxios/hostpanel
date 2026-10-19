using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Admin.Queries.GetServiceUsageReport;

/// <summary>
/// Handles <see cref="GetServiceUsageReportQuery"/> by counting client services grouped by product name.
/// </summary>
public sealed class GetServiceUsageReportHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetServiceUsageReportHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Returns service usage counts grouped by product name, ordered by count descending.
    /// </summary>
    /// <param name="query">The service usage report query.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with the usage items. This use case refuses nothing.</returns>
    public async Task<Result<IReadOnlyList<ServiceUsageReportItemDto>>> HandleAsync(
        GetServiceUsageReportQuery query, CancellationToken ct)
    {
        // Was IClientServiceRepository.GetAllAsync -- read-only, so no tracking is needed.
        var services = await _context.ClientServices.AsNoTracking().ToListAsync(ct);

        var grouped = services
            .GroupBy(s => s.ProductId)
            .Select(g => new { ProductId = g.Key, Count = g.Count() })
            .ToList();

        // Was IProductRepository.FindByIdsAsync -- read-only, so no tracking is needed.
        var productIds = grouped.Select(g => g.ProductId).ToList();
        var products = await _context.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(ct);
        var productMap = products.ToDictionary(p => p.Id, p => p.Name);

        IReadOnlyList<ServiceUsageReportItemDto> items = grouped
            .Select(g => new ServiceUsageReportItemDto(
                productMap.TryGetValue(g.ProductId, out var name) ? name : null,
                g.Count))
            .OrderByDescending(r => r.Count)
            .ToList();

        return Result<IReadOnlyList<ServiceUsageReportItemDto>>.Success(items);
    }
}
