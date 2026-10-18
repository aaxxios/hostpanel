using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Admin.Queries.GetClientGrowthReport;

/// <summary>
/// Handles <see cref="GetClientGrowthReportQuery"/> by grouping new clients by registration date.
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
    /// Returns daily new client counts for the requested date range.
    /// </summary>
    /// <param name="query">The client growth report query with start and end dates.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success with the daily client growth items, ordered by date. A start after the end simply
    /// answers an empty report; this use case refuses nothing.
    /// </returns>
    public async Task<Result<IReadOnlyList<ClientGrowthReportItemDto>>> HandleAsync(
        GetClientGrowthReportQuery query, CancellationToken ct)
    {
        // Was IClientRepository.GetCreatedBetweenAsync: both bounds inclusive, ordered by
        // CreatedAt ascending -- the grouping below only needs a set, but the read-only path
        // keeps the same shape the repository handed back.
        var clients = await _context.Clients
            .AsNoTracking()
            .Where(c => c.CreatedAt >= query.StartDate && c.CreatedAt <= query.EndDate)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(ct);

        IReadOnlyList<ClientGrowthReportItemDto> items = clients
            .GroupBy(c => DateOnly.FromDateTime(c.CreatedAt.UtcDateTime))
            .Select(g => new ClientGrowthReportItemDto(g.Key, g.Count()))
            .OrderBy(r => r.Date)
            .ToList();

        return Result<IReadOnlyList<ClientGrowthReportItemDto>>.Success(items);
    }
}
