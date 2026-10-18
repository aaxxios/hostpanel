using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Migration.Commands.DeleteMigrationJob;

/// <summary>Deletes a migration job the operator no longer wants to keep a record of.</summary>
public sealed class DeleteMigrationJobHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public DeleteMigrationJobHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The job to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>MIGRATION_JOB_NOT_FOUND</c> when the id names no job.</returns>
    public async Task<Result> HandleAsync(DeleteMigrationJobCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var job = await _context.MigrationJobs.FirstOrDefaultAsync(j => j.Id == cmd.JobId, ct);
        if (job is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.MIGRATION_JOB_NOT_FOUND);
        }

        _context.MigrationJobs.Remove(job);
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
