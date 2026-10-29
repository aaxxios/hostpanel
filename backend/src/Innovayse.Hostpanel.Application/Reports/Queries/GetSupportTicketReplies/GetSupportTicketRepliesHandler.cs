using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Reports.Common;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Reports.Queries.GetSupportTicketReplies;

/// <summary>Handles <see cref="GetSupportTicketRepliesQuery"/>.</summary>
public sealed class GetSupportTicketRepliesHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>The source of the current moment.</summary>
    private readonly IClock _clock;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="clock">The source of the current moment.</param>
    public GetSupportTicketRepliesHandler(IHostpanelDbContext context, IClock clock)
    {
        _context = context;
        _clock = clock;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">Year and month to report on; either may be omitted.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Reply counts per admin per day for the month. Never refuses.</returns>
    public async Task<Result<SupportTicketRepliesDto>> HandleAsync(
        GetSupportTicketRepliesQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        // The period defaults here rather than at the endpoint so every caller of the query,
        // HTTP or not, lands on the same month.
        var period = ReportMonth.Resolve(query.Year, query.Month, _clock.UtcNow);
        var year = period.Year;
        var month = period.Month;
        var from = period.From;
        var to = period.To;
        var daysInMonth = period.DaysInMonth;

        var replies = await _context.Tickets.AsNoTracking()
            .SelectMany(t => t.Replies)
            .Where(r => r.IsStaffReply && r.CreatedAt >= from && r.CreatedAt < to)
            .Select(r => new { r.AuthorName, Day = r.CreatedAt.Day })
            .ToListAsync(ct);

        var grouped = replies
            .GroupBy(r => r.AuthorName)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var byDay = g.GroupBy(r => r.Day).ToDictionary(x => x.Key, x => x.Count());
                var dailyCounts = Enumerable.Range(1, daysInMonth)
                    .Select(d => byDay.GetValueOrDefault(d, 0))
                    .ToList();
                return new SupportTicketRepliesRowDto(g.Key, dailyCounts, g.Count());
            })
            .ToList();

        return Result<SupportTicketRepliesDto>.Success(new SupportTicketRepliesDto(month, year, daysInMonth, grouped));
    }
}
