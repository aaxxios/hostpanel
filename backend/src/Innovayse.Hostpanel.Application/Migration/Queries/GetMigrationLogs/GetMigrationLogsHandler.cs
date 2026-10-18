using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Extensions;
using Innovayse.Hostpanel.Domain.Migration;
using Innovayse.Hostpanel.Domain.Migration.Enums;
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
    /// The job id, the two optional filters — already checked against their enums by
    /// <see cref="GetMigrationLogsValidator"/> — and the page to return.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success with one page of log entries and the unpaged total. An id that names no job answers
    /// an empty page rather than a refusal, the same as before this conversion.
    /// </returns>
    public async Task<Result<MigrationLogPageDto>> HandleAsync(GetMigrationLogsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var action = query.Action is null
            ? (MigrationLogAction?)null
            : Enum.Parse<MigrationLogAction>(query.Action, true);
        var entityType = query.EntityType is null
            ? (MigrationEntityType?)null
            : Enum.Parse<MigrationEntityType>(query.EntityType, true);

        var filtered = _context.MigrationLogs.AsNoTracking().Where(l => l.JobId == query.JobId);
        if (action.HasValue)
        {
            filtered = filtered.Where(l => l.Action == action.Value);
        }

        if (entityType.HasValue)
        {
            filtered = filtered.Where(l => l.EntityType == entityType.Value);
        }

        var total = await filtered.CountAsync(ct);
        var items = await filtered
            .OrderBy(l => l.CreatedAt)
            .Paginate(query.Page, query.PageSize)
            .ToListAsync(ct);

        return Result<MigrationLogPageDto>.Success(new MigrationLogPageDto(
            items.Select(l => l.ToDto()).ToList(), total, query.Page, query.PageSize));
    }
}
