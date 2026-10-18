using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Reports.Common;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Reports.Queries.GetDailyTransactions;

/// <summary>Handles <see cref="GetDailyTransactionsQuery"/>.</summary>
public sealed class GetDailyTransactionsHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetDailyTransactionsHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The month to report on; defaults to the current UTC year and month.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>One row per day of the month, zero-filled where nothing was recorded. Never refuses.</returns>
    public async Task<Result<MonthlyTransactionsReportDto>> HandleAsync(
        GetDailyTransactionsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var period = ReportMonth.Resolve(query.Year, query.Month, DateTimeOffset.UtcNow);
        var year = period.Year;
        var month = period.Month;
        var from = period.From;
        var to = period.To;

        var dailyRows = await _context.Transactions
            .AsNoTracking()
            .Where(t => t.Date >= from && t.Date < to)
            .GroupBy(t => t.Date.Date)
            .Select(g => new
            {
                Date = g.Key,
                AmountIn = g.Sum(t => t.AmountIn),
                Fees = g.Sum(t => t.Fees),
                AmountOut = g.Sum(t => t.AmountOut),
            })
            .OrderBy(r => r.Date)
            .ToListAsync(ct);

        var map = dailyRows.ToDictionary(r => r.Date.Date);

        var daysInMonth = period.DaysInMonth;
        var rows = new List<DailyTransactionDto>();
        for (var day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
            map.TryGetValue(date, out var r);
            var amountIn = r?.AmountIn ?? 0m;
            var fees = r?.Fees ?? 0m;
            var amountOut = r?.AmountOut ?? 0m;

            // Balance is the daily net: money in, minus fees, minus money out.
            var balance = amountIn - fees - amountOut;
            rows.Add(new DailyTransactionDto(date.ToString("yyyy-MM-dd"), amountIn, fees, amountOut, balance));
        }

        var dto = new MonthlyTransactionsReportDto(
            month, year, rows,
            rows.Sum(r => r.AmountIn),
            rows.Sum(r => r.Fees),
            rows.Sum(r => r.AmountOut));

        return Result<MonthlyTransactionsReportDto>.Success(dto);
    }
}
