using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Reports.Common;
using Innovayse.Hostpanel.Domain.Orders;
using Innovayse.Hostpanel.Domain.Orders.Enums;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Reports.Queries.GetMonthlyOrders;

/// <summary>Handles <see cref="GetMonthlyOrdersQuery"/>.</summary>
public sealed class GetMonthlyOrdersHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetMonthlyOrdersHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">Year and month to report on; either may be omitted.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Orders grouped by product group, with group and overall totals. Never refuses.</returns>
    public async Task<Result<MonthlyOrdersDto>> HandleAsync(GetMonthlyOrdersQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        // The period defaults here rather than at the endpoint so every caller of the query,
        // HTTP or not, lands on the same month.
        var period = ReportMonth.Resolve(query.Year, query.Month, DateTimeOffset.UtcNow);
        var year = period.Year;
        var month = period.Month;
        var from = period.From;
        var to = period.To;

        var orderItems = await _context.Orders.AsNoTracking()
            .Where(o => o.CreatedAt >= from && o.CreatedAt < to && o.Status == OrderStatus.Active)
            .SelectMany(o => o.Items)
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, Units = g.Count(), Value = g.Sum(i => i.FirstPaymentAmount) })
            .ToListAsync(ct);

        var salesMap = orderItems.ToDictionary(x => x.ProductId, x => new { x.Units, x.Value });

        var groups = await _context.ProductGroups.AsNoTracking()
            .Where(g => g.IsActive)
            .Include(g => g.Products.Where(p => p.Status == ProductStatus.Active))
            .OrderBy(g => g.Name)
            .ToListAsync(ct);

        var result = groups.Select(g =>
        {
            var products = g.Products.Select(p =>
            {
                var sale = salesMap.GetValueOrDefault(p.Id);
                return new MonthlyOrderProductDto(p.Id, p.Name, sale?.Units ?? 0, sale?.Value ?? 0m);
            }).ToList();

            return new MonthlyOrderGroupDto(
                g.Name,
                products,
                products.Sum(p => p.UnitsSold),
                products.Sum(p => p.Value));
        }).ToList();

        var dto = new MonthlyOrdersDto(
            month, year, result,
            result.Sum(g => g.GroupUnitsSold),
            result.Sum(g => g.GroupValue));

        return Result<MonthlyOrdersDto>.Success(dto);
    }
}
