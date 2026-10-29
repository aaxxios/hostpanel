using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Common;
using Innovayse.Hostpanel.Application.Migration.Extensions;
using Innovayse.Hostpanel.Shared.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Migration.Queries.GetMigrationLogs;

/// <summary>Returns one page of a migration job's import report, optionally filtered.</summary>
public sealed class GetMigrationLogsHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public GetMigrationLogsHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the query.
    /// </summary>
    /// <param name="query">
    /// The job id, the two optional filters, already checked against their enums by
    /// <see cref="GetMigrationLogsValidator"/>, and the page to return.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success with one page of log entries and the unpaged total. An id that names no job answers
    /// an empty page rather than a refusal.
    /// </returns>
    public async Task<Result<PaginatedList<MigrationLogDto>>> HandleAsync(GetMigrationLogsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var filtered = _context.MigrationLogs.AsNoTracking().Where(l => l.JobId == query.JobId);
        if (query.Action is { } action)
        {
            filtered = filtered.Where(l => l.Action == action);
        }

        if (query.EntityType is { } entityType)
        {
            filtered = filtered.Where(l => l.EntityType == entityType);
        }

        var total = await filtered.CountAsync(ct);
        var items = await filtered
            .OrderBy(l => l.CreatedAt)
            .Paginate(query.Page, query.PageSize)
            .ToListAsync(ct);

        return Result<PaginatedList<MigrationLogDto>>.Success(new PaginatedList<MigrationLogDto>(
            items.Select(l => l.ToDto()).ToList(), total, query.Page, query.PageSize));
    }
}
