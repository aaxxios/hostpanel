using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Reports.Common;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Reports.Queries.GetClientGrowthReport;

/// <summary>
/// Handles <see cref="GetClientGrowthReportQuery"/> by counting new clients per day in the database.
/// </summary>
public sealed class GetClientGrowthReportHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetClientGrowthReportHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Returns daily new client counts for the requested window.
    /// </summary>
    /// <param name="query">The client growth report query with its start and end.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success with one item per day that had a sign-up, ordered by date. A start after the end simply
    /// answers an empty report; this use case refuses nothing.
    /// </returns>
    public async Task<Result<IReadOnlyList<ClientGrowthReportItemDto>>> HandleAsync(
        GetClientGrowthReportQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var counts = await _context.Clients
            .AsNoTracking()
            .Where(c => c.CreatedAt >= query.StartDate && c.CreatedAt < query.EndDate)
            .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month, c.CreatedAt.Day })
            .Select(g => new { g.Key.Year, g.Key.Month, g.Key.Day, Count = g.Count() })
            .ToListAsync(ct);

        IReadOnlyList<ClientGrowthReportItemDto> items = [.. counts
            .Select(c => new ClientGrowthReportItemDto(new DateOnly(c.Year, c.Month, c.Day), c.Count))
            .OrderBy(r => r.Date)];

        return Result<IReadOnlyList<ClientGrowthReportItemDto>>.Success(items);
    }
}
