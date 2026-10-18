using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Common;
using Innovayse.Hostpanel.Application.Migration.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Innovayse.Hostpanel.Application.Migration.Queries.TestMigrationConnection;

/// <summary>Pings the migration plugin at the job's configured source, to prove the two can talk.</summary>
public sealed class TestMigrationConnectionHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>The legacy system's migration plugin — a port this deployment does not own.</summary>
    private readonly IMigrationSource _source;

    /// <summary>Logs the source's own failure detail, which never reaches the response.</summary>
    private readonly ILogger<TestMigrationConnectionHandler> _logger;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="source">The legacy system's migration plugin.</param>
    /// <param name="logger">Logs the source's own failure detail.</param>
    public TestMigrationConnectionHandler(
        IHostpanelDbContext context, IMigrationSource source, ILogger<TestMigrationConnectionHandler> logger)
    {
        _context = context;
        _source = source;
        _logger = logger;
    }

    /// <summary>
    ///     Handles the query.
    /// </summary>
    /// <param name="query">The job whose connection to test.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success with the job's state once the plugin has answered, <c>MIGRATION_JOB_NOT_FOUND</c>
    /// when the id names no job, or <c>MIGRATION_CONNECTION_UPSTREAM_FAILED</c> when the source
    /// could not be reached or refused the key — no detail from the source reaches the response.
    /// </returns>
    public async Task<Result<MigrationJobDto>> HandleAsync(TestMigrationConnectionQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var job = await _context.MigrationJobs.FirstOrDefaultAsync(j => j.Id == query.Id, ct);
        if (job is null)
        {
            return Result<MigrationJobDto>.FailureCode(HostpanelErrorCodes.MIGRATION_JOB_NOT_FOUND);
        }

        try
        {
            await _source.PingAsync(job.SourceUrl, job.Key, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // The source is a port this deployment does not own (rules/security.md: no upstream
            // detail in a response). What it actually said — a connection refusal, a non-success
            // status, an unreadable body — is only useful to whoever can fix the source or the
            // configured address, which is the operator reading this log, not the caller.
            _logger.LogWarning(
                ex, "Migration connection test failed for job {JobId} at {SourceUrl}.", job.Id, job.SourceUrl);
            return Result<MigrationJobDto>.FailureCode(HostpanelErrorCodes.MIGRATION_CONNECTION_UPSTREAM_FAILED);
        }

        job.RecordPing();
        await _context.SaveChangesAsync(ct);

        return Result<MigrationJobDto>.Success(job.ToDto());
    }
}
