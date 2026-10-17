using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Reports.Queries.DailyPerformance;

/// <summary>Handles <see cref="DailyPerformanceQuery"/>.</summary>
public sealed class DailyPerformanceHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public DailyPerformanceHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The date range to report on; defaults to the trailing 30 days.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>One row per day in the range. Never refuses.</returns>
    public async Task<Result<IReadOnlyList<DailyPerformanceDto>>> HandleAsync(
        DailyPerformanceQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var to = query.To ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var from = query.From ?? to.AddDays(-29);
        var fromUtc = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toUtc = to.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        var newInvoices = await _context.Invoices
            .AsNoTracking()
            .Where(i => i.CreatedAt >= fromUtc && i.CreatedAt <= toUtc)
            .GroupBy(i => new { i.CreatedAt.Year, i.CreatedAt.Month, i.CreatedAt.Day })
            .Select(g => new { Date = new { g.Key.Year, g.Key.Month, g.Key.Day }, Count = g.Count() })
            .ToListAsync(ct);

        var paidInvoices = await _context.Invoices
            .AsNoTracking()
            .Where(i => i.Status == InvoiceStatus.Paid && i.CreatedAt >= fromUtc && i.CreatedAt <= toUtc)
            .GroupBy(i => new { i.CreatedAt.Year, i.CreatedAt.Month, i.CreatedAt.Day })
            .Select(g => new { Date = new { g.Key.Year, g.Key.Month, g.Key.Day }, Count = g.Count() })
            .ToListAsync(ct);

        var orders = await _context.Orders
            .AsNoTracking()
            .Where(o => o.CreatedAt >= fromUtc && o.CreatedAt <= toUtc)
            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month, o.CreatedAt.Day })
            .Select(g => new { Date = new { g.Key.Year, g.Key.Month, g.Key.Day }, Count = g.Count() })
            .ToListAsync(ct);

        var cancellations = await _context.CancellationRequests
            .AsNoTracking()
            .Where(c => c.CreatedAt >= fromUtc && c.CreatedAt <= toUtc)
            .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month, c.CreatedAt.Day })
            .Select(g => new { Date = new { g.Key.Year, g.Key.Month, g.Key.Day }, Count = g.Count() })
            .ToListAsync(ct);

        var openedTickets = await _context.Tickets
            .AsNoTracking()
            .Where(t => t.CreatedAt >= fromUtc && t.CreatedAt <= toUtc)
            .GroupBy(t => new { t.CreatedAt.Year, t.CreatedAt.Month, t.CreatedAt.Day })
            .Select(g => new { Date = new { g.Key.Year, g.Key.Month, g.Key.Day }, Count = g.Count() })
            .ToListAsync(ct);

        var days = Enumerable.Range(0, to.DayNumber - from.DayNumber + 1).Select(i => from.AddDays(i));

        IReadOnlyList<DailyPerformanceDto> rows = days.Select(day => new DailyPerformanceDto(
            Date: day.ToString("yyyy-MM-dd"),
            CompletedOrders: orders.FirstOrDefault(x => x.Date.Year == day.Year && x.Date.Month == day.Month && x.Date.Day == day.Day)?.Count ?? 0,
            NewInvoices: newInvoices.FirstOrDefault(x => x.Date.Year == day.Year && x.Date.Month == day.Month && x.Date.Day == day.Day)?.Count ?? 0,
            PaidInvoices: paidInvoices.FirstOrDefault(x => x.Date.Year == day.Year && x.Date.Month == day.Month && x.Date.Day == day.Day)?.Count ?? 0,
            OpenedTickets: openedTickets.FirstOrDefault(x => x.Date.Year == day.Year && x.Date.Month == day.Month && x.Date.Day == day.Day)?.Count ?? 0,
            // The old repository never counted replies here either; a reply total needs the
            // ticket-replies aggregate this report does not otherwise compute. Kept at zero
            // rather than invented, matching the pre-conversion behaviour exactly.
            TicketReplies: 0,
            CancellationRequests: cancellations.FirstOrDefault(x => x.Date.Year == day.Year && x.Date.Month == day.Month && x.Date.Day == day.Day)?.Count ?? 0
        )).ToList();

        return Result<IReadOnlyList<DailyPerformanceDto>>.Success(rows);
    }
}
