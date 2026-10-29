using Innovayse.Hostpanel.Application.Billing.Extensions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Reports.Common;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Reports.Queries.GetRevenueReport;

/// <summary>
/// Handles <see cref="GetRevenueReportQuery"/> by summing income per day in the database.
/// </summary>
public sealed class GetRevenueReportHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetRevenueReportHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Returns daily income totals for the requested window, in the base currency.
    /// </summary>
    /// <param name="query">The revenue report query with its start and end.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success with one item per day that had income, ordered by date. A start after the end simply
    /// answers an empty report; this use case refuses nothing.
    /// </returns>
    public async Task<Result<IReadOnlyList<RevenueReportItemDto>>> HandleAsync(
        GetRevenueReportQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        // The database sums per currency and day; only those rows come back to be converted.
        var sums = await _context.IncomeEntries()
            .Within(query.StartDate, query.EndDate)
            .GroupBy(e => new { e.Currency, e.At.Year, e.At.Month, e.At.Day })
            .Select(g => new { g.Key.Currency, g.Key.Year, g.Key.Month, g.Key.Day, Amount = g.Sum(e => e.Amount) })
            .ToListAsync(ct);
        var rates = await _context.GetBaseCurrencyRatesAsync(ct);

        IReadOnlyList<RevenueReportItemDto> items = [.. sums
            .GroupBy(s => new DateOnly(s.Year, s.Month, s.Day))
            .Select(g => new RevenueReportItemDto(g.Key, g.Sum(s => rates.ToBase(s.Amount, s.Currency)), rates.Currency))
            .OrderBy(r => r.Date)];

        return Result<IReadOnlyList<RevenueReportItemDto>>.Success(items);
    }
}
