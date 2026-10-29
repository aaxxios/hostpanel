using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Application.Support.Common.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Queries.GetSupportOverview;

/// <summary>
/// Aggregates support overview statistics for a given time period: ticket counts, reply counts,
/// average first response time, and tickets-by-hour distribution.
/// </summary>
public sealed class GetSupportOverviewHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>The source of the current moment.</summary>
    private readonly IClock _clock;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="clock">The source of the current moment.</param>
    public GetSupportOverviewHandler(IHostpanelDbContext context, IClock clock)
    {
        _context = context;
        _clock = clock;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The period to aggregate over.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success with the aggregated stats. This use case refuses nothing; the period is validated
    /// before it reaches here.
    /// </returns>
    public async Task<Result<SupportOverviewDto>> HandleAsync(GetSupportOverviewQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var (from, to) = ResolvePeriod(query.Period, _clock.UtcNow);
        var tickets = await _context.Tickets
            .AsNoTracking()
            .Include(t => t.Replies)
            .Where(t => t.CreatedAt >= from && t.CreatedAt < to)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

        var newTickets = tickets.Count;

        var clientReplies = tickets
            .SelectMany(t => t.Replies)
            .Count(r => !r.IsStaffReply && r.CreatedAt >= from && r.CreatedAt < to);

        var staffReplies = tickets
            .SelectMany(t => t.Replies)
            .Count(r => r.IsStaffReply && r.CreatedAt >= from && r.CreatedAt < to);

        var ticketsWithoutReply = tickets
            .Count(t => !t.Replies.Any(r => r.IsStaffReply));

        var averageFirstResponse = ComputeAverageFirstResponse(tickets);

        var ticketsByHour = new int[24];
        foreach (var ticket in tickets)
        {
            ticketsByHour[ticket.CreatedAt.Hour]++;
        }

        var dto = new SupportOverviewDto(
            newTickets, clientReplies, staffReplies, ticketsWithoutReply, averageFirstResponse, ticketsByHour);

        return Result.Success(dto);
    }

    /// <summary>Computes the average time to first staff response, formatted for display.</summary>
    /// <param name="tickets">The tickets in the period.</param>
    /// <returns>The formatted average, or <see langword="null"/> when no ticket has a staff reply.</returns>
    private static string? ComputeAverageFirstResponse(IReadOnlyList<Domain.Support.Ticket> tickets)
    {
        var firstResponseTimes = tickets
            .Select(t =>
            {
                var firstStaffReply = t.Replies
                    .Where(r => r.IsStaffReply)
                    .OrderBy(r => r.CreatedAt)
                    .FirstOrDefault();
                return firstStaffReply != null ? (TimeSpan?)(firstStaffReply.CreatedAt - t.CreatedAt) : null;
            })
            .Where(ts => ts.HasValue)
            .Select(ts => ts!.Value)
            .ToList();

        if (firstResponseTimes.Count == 0)
        {
            return null;
        }

        var avgTicks = (long)firstResponseTimes.Average(ts => ts.Ticks);
        return FormatTimeSpan(TimeSpan.FromTicks(avgTicks));
    }

    /// <summary>Resolves a period into a date range.</summary>
    /// <param name="period">The reporting period.</param>
    /// <param name="now">The current moment the window ends at.</param>
    /// <returns>The date boundaries of the period.</returns>
    private static ReportingWindow ResolvePeriod(SupportOverviewPeriod period, DateTimeOffset now)
    {
        var todayStart = new DateTimeOffset(now.Date, TimeSpan.Zero);

        return period switch
        {
            SupportOverviewPeriod.Yesterday => new ReportingWindow(todayStart.AddDays(-1), todayStart),
            SupportOverviewPeriod.Last7Days => new ReportingWindow(todayStart.AddDays(-7), now),
            SupportOverviewPeriod.Last30Days => new ReportingWindow(todayStart.AddDays(-30), now),
            _ => new ReportingWindow(todayStart, now), // Today or default
        };
    }

    /// <summary>Formats a <see cref="TimeSpan"/> as a human-readable string (e.g. "2h 15m").</summary>
    /// <param name="ts">The time span to format.</param>
    /// <returns>Formatted string.</returns>
    private static string FormatTimeSpan(TimeSpan ts)
    {
        if (ts.TotalDays >= 1)
        {
            return $"{(int)ts.TotalDays}d {ts.Hours}h {ts.Minutes}m";
        }

        if (ts.TotalHours >= 1)
        {
            return $"{(int)ts.TotalHours}h {ts.Minutes}m";
        }

        return $"{ts.Minutes}m";
    }
}
