using Innovayse.Hostpanel.Application.Billing.Extensions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Reports.Common;
using Innovayse.Hostpanel.Domain.Orders.Enums;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Reports.Queries.GetMonthlyOrders;

/// <summary>Handles <see cref="GetMonthlyOrdersQuery"/>.</summary>
public sealed class GetMonthlyOrdersHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>The source of the current moment.</summary>
    private readonly IClock _clock;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="clock">The source of the current moment.</param>
    public GetMonthlyOrdersHandler(IHostpanelDbContext context, IClock clock)
    {
        _context = context;
        _clock = clock;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">Year and month to report on; either may be omitted.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Units ordered and their value per product, grouped by product group, with group and overall totals in the base currency. Never refuses.</returns>
    public async Task<Result<MonthlyOrdersDto>> HandleAsync(GetMonthlyOrdersQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        // The period defaults here rather than at the endpoint so every caller of the query,
        // HTTP or not, lands on the same month.
        var period = ReportMonth.Resolve(query.Year, query.Month, _clock.UtcNow);
        var year = period.Year;
        var month = period.Month;
        var from = period.From;
        var to = period.To;

        // A unit is one order line: an order line carries no quantity of its own. Every order
        // placed in the month counts, pending or accepted; a cancelled or fraud order sold nothing.
        // Orders are priced in the client's currency, so the database sums per product and
        // currency and the sums are converted to the base currency before they are added.
        var sums = await _context.Orders.AsNoTracking()
            .Where(o => o.CreatedAt >= from && o.CreatedAt < to
                && o.Status != OrderStatus.Cancelled && o.Status != OrderStatus.Fraud)
            .SelectMany(o => o.Items, (o, i) => new { i.ProductId, o.Currency, i.FirstPaymentAmount })
            .GroupBy(x => new { x.ProductId, x.Currency })
            .Select(g => new { g.Key.ProductId, g.Key.Currency, Units = g.Count(), Value = g.Sum(x => x.FirstPaymentAmount) })
            .ToListAsync(ct);
        var rates = await _context.GetBaseCurrencyRatesAsync(ct);

        var salesMap = sums
            .GroupBy(x => x.ProductId)
            .ToDictionary(
                g => g.Key,
                g => new { Units = g.Sum(x => x.Units), Value = g.Sum(x => rates.ToBase(x.Value, x.Currency)) });

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
            result.Sum(g => g.GroupValue),
            rates.Currency);

        return Result<MonthlyOrdersDto>.Success(dto);
    }
}
