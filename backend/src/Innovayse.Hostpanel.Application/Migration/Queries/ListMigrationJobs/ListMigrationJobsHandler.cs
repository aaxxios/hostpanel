using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Common;
using Innovayse.Hostpanel.Application.Migration.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Migration.Queries.ListMigrationJobs;

/// <summary>Lists every migration job, most recently created first.</summary>
public sealed class ListMigrationJobsHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public ListMigrationJobsHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the query.
    /// </summary>
    /// <param name="query">Carries no input.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with every migration job. Never refuses.</returns>
    public async Task<Result<IReadOnlyList<MigrationJobDto>>> HandleAsync(
        ListMigrationJobsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var jobs = await _context.MigrationJobs
            .AsNoTracking()
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(ct);

        return Result<IReadOnlyList<MigrationJobDto>>.Success(jobs.Select(j => j.ToDto()).ToList());
    }
}
