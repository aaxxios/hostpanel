using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Queries.GetClientTicketStats;

/// <summary>Computes ticket statistics for a client broken down by time period.</summary>
public sealed class GetClientTicketStatsHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>The source of the current moment.</summary>
    private readonly IClock _clock;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="clock">The source of the current moment.</param>
    public GetClientTicketStatsHandler(IHostpanelDbContext context, IClock clock)
    {
        _context = context;
        _clock = clock;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The client to count tickets for.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success with the client's ticket counts. This use case refuses nothing: a client with no
    /// tickets simply gets zeros.
    /// </returns>
    public async Task<Result<ClientTicketStatsDto>> HandleAsync(
        GetClientTicketStatsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var now = _clock.UtcNow;
        var thisMonthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var lastMonthStart = thisMonthStart.AddMonths(-1);
        var thisYearStart = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var lastYearStart = new DateTimeOffset(now.Year - 1, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var lastYearEnd = thisYearStart;

        var thisMonth = await CountAsync(query.ClientId, thisMonthStart, now, ct);
        var lastMonth = await CountAsync(query.ClientId, lastMonthStart, thisMonthStart, ct);
        var thisYear = await CountAsync(query.ClientId, thisYearStart, now, ct);
        var lastYear = await CountAsync(query.ClientId, lastYearStart, lastYearEnd, ct);

        return Result.Success(new ClientTicketStatsDto(thisMonth, lastMonth, thisYear, lastYear));
    }

    /// <summary>Counts a client's tickets opened within a date range.</summary>
    /// <param name="clientId">FK to the client.</param>
    /// <param name="from">Start of the date range (inclusive).</param>
    /// <param name="to">End of the date range (exclusive).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The number of matching tickets.</returns>
    private Task<int> CountAsync(int clientId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct)
    {
        return _context.Tickets
            .AsNoTracking()
            .CountAsync(t => t.ClientId == clientId && t.CreatedAt >= from && t.CreatedAt < to, ct);
    }
}
