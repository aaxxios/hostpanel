using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Common;
using Innovayse.Hostpanel.Application.Migration.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Migration.Queries.GetMigrationStatus;

/// <summary>Returns the current status and progress of one migration job.</summary>
public sealed class GetMigrationStatusHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public GetMigrationStatusHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the query.
    /// </summary>
    /// <param name="query">The job to report on.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with the job's state, or <c>MIGRATION_JOB_NOT_FOUND</c> when the id names no job.</returns>
    public async Task<Result<MigrationJobDto>> HandleAsync(GetMigrationStatusQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var job = await _context.MigrationJobs.AsNoTracking().FirstOrDefaultAsync(j => j.Id == query.JobId, ct);
        if (job is null)
        {
            return Result<MigrationJobDto>.FailureCode(HostpanelErrorCodes.MIGRATION_JOB_NOT_FOUND);
        }

        return Result<MigrationJobDto>.Success(job.ToDto());
    }
}
