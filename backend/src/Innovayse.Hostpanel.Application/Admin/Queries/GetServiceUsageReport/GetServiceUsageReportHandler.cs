using Innovayse.Hostpanel.Application.Admin.Common;
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
        // One query: the services are left-joined to their product and the database counts per product; a service
        // whose product no longer exists is still counted, with a null name.
        var rows = await (
            from service in _context.ClientServices.AsNoTracking()
            join product in _context.Products.AsNoTracking() on service.ProductId equals product.Id into products
            from product in products.DefaultIfEmpty()
            group service by new { service.ProductId, Name = product == null ? null : product.Name } into g
            select new { g.Key.Name, Count = g.Count() })
            .ToListAsync(ct);

        IReadOnlyList<ServiceUsageReportItemDto> items = rows
            .Select(r => new ServiceUsageReportItemDto(r.Name, r.Count))
            .OrderByDescending(r => r.Count)
            .ToList();

        return Result<IReadOnlyList<ServiceUsageReportItemDto>>.Success(items);
    }
}
