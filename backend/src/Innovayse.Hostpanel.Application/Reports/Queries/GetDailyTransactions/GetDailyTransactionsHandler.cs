using Innovayse.Hostpanel.Application.Billing.Extensions;
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

    /// <summary>The source of the current moment.</summary>
    private readonly IClock _clock;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="clock">The source of the current moment.</param>
    public GetDailyTransactionsHandler(IHostpanelDbContext context, IClock clock)
    {
        _context = context;
        _clock = clock;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The month to report on; defaults to the current UTC year and month.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>One row per day of the month, zero-filled where nothing was recorded, in the base currency. Never refuses.</returns>
    public async Task<Result<MonthlyTransactionsReportDto>> HandleAsync(
        GetDailyTransactionsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var period = ReportMonth.Resolve(query.Year, query.Month, _clock.UtcNow);
        var year = period.Year;
        var month = period.Month;
        var from = period.From;
        var to = period.To;

        // The ledger holds each client's own currency, so the database sums per currency and day
        // and the few sums are converted to the base currency before they are added together.
        var sums = await _context.Transactions
            .AsNoTracking()
            .Where(t => t.Date >= from && t.Date < to)
            .GroupBy(t => new { t.Currency, t.Date.Day })
            .Select(g => new
            {
                g.Key.Currency,
                g.Key.Day,
                AmountIn = g.Sum(t => t.AmountIn),
                Fees = g.Sum(t => t.Fees),
                AmountOut = g.Sum(t => t.AmountOut),
            })
            .ToListAsync(ct);
        var rates = await _context.GetBaseCurrencyRatesAsync(ct);

        var map = sums
            .GroupBy(s => s.Day)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    AmountIn = g.Sum(s => rates.ToBase(s.AmountIn, s.Currency)),
                    Fees = g.Sum(s => rates.ToBase(s.Fees, s.Currency)),
                    AmountOut = g.Sum(s => rates.ToBase(s.AmountOut, s.Currency)),
                });

        var daysInMonth = period.DaysInMonth;
        var rows = new List<DailyTransactionDto>();
        for (var day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
            map.TryGetValue(day, out var r);
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
            rows.Sum(r => r.AmountOut),
            rates.Currency);

        return Result<MonthlyTransactionsReportDto>.Success(dto);
    }
}
