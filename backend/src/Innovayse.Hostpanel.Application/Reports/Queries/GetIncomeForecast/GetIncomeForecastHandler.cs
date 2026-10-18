using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Reports.Queries.GetIncomeForecast;

/// <summary>Handles <see cref="GetIncomeForecastQuery"/>.</summary>
public sealed class GetIncomeForecastHandler
{
    /// <summary>Months in the trailing window the quarterly column sums over.</summary>
    private const int QuarterWindow = 3;

    /// <summary>Months in the trailing window the semi-annual column sums over.</summary>
    private const int SemiAnnualWindow = 6;

    /// <summary>Three-letter month names, indexed 0 for January.</summary>
    private static readonly string[] MonthNames =
        ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetIncomeForecastHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Projects the year's monthly paid-invoice income into the rolling windows the forecast
    /// screen shows.
    /// </summary>
    /// <param name="query">Year to forecast; may be omitted.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>One row per month of the year, in calendar order. Never refuses.</returns>
    public async Task<Result<IReadOnlyList<IncomeForecastRowDto>>> HandleAsync(
        GetIncomeForecastQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var year = query.Year ?? DateTime.UtcNow.Year;

        var monthlyTotals = await _context.Invoices
            .AsNoTracking()
            .Where(i => i.Status == InvoiceStatus.Paid && i.CreatedAt.Year == year)
            .GroupBy(i => i.CreatedAt.Month)
            .Select(g => new { Month = g.Key, Total = g.Sum(i => i.Total) })
            .ToListAsync(ct);

        var monthly = Enumerable.Range(1, 12)
            .Select(m => monthlyTotals.FirstOrDefault(x => x.Month == m)?.Total ?? 0m)
            .ToList();

        // Each window is trailing and clamped at January, so the early months sum fewer than
        // their nominal length rather than wrapping into the previous year.
        var annual = monthly.Sum();
        IReadOnlyList<IncomeForecastRowDto> rows = [.. monthly.Select((amount, index) => new IncomeForecastRowDto(
            MonthNames[index],
            amount,
            monthly.Skip(Math.Max(0, index - (QuarterWindow - 1))).Take(QuarterWindow).Sum(),
            monthly.Skip(Math.Max(0, index - (SemiAnnualWindow - 1))).Take(SemiAnnualWindow).Sum(),
            annual,
            monthly.Take(index + 1).Sum()))];

        return Result<IReadOnlyList<IncomeForecastRowDto>>.Success(rows);
    }
}
