using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Scheduling;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Innovayse.Hostpanel.Infrastructure.Security;
using Innovayse.Hostpanel.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Infrastructure.Scheduling;

/// <summary>
/// <see cref="IJobLease"/> over the <c>JobRuns</c> table. The lease is taken by saving the
/// <see cref="JobRun"/> row: the row carries PostgreSQL's <c>xmin</c> as its concurrency token, and a
/// first insert is guarded by the key, so of two instances that try at the same moment exactly one
/// save succeeds and the other is refused.
/// </summary>
/// <remarks>
/// Each call works on a context of its own, opened on the same database and without the request
/// scope's interceptors. The scoped context belongs to the pass that is running: saving through it
/// would write that pass's half-finished changes, and a failed lease save must not leave state behind
/// in it.
/// </remarks>
public sealed class JobLease : IJobLease
{
    /// <summary>The scoped context, used only to learn which database to open.</summary>
    private readonly AppDbContext _scoped;

    /// <summary>The service the contexts this class opens encrypt their protected columns with.</summary>
    private readonly IEncryptionService _encryption;

    /// <summary>Initializes a new instance of the <see cref="JobLease"/> class.</summary>
    /// <param name="scoped">The scoped context, used only to learn which database to open.</param>
    /// <param name="encryption">The service the contexts this class opens encrypt their protected columns with.</param>
    public JobLease(AppDbContext scoped, IEncryptionService encryption)
    {
        _scoped = scoped;
        _encryption = encryption;
    }

    /// <inheritdoc/>
    public async Task<JobLeaseDecision> TryAcquireAsync(JobLeaseClaim claim, CancellationToken ct)
    {
        await using var db = Open();

        var run = await db.JobRuns.SingleOrDefaultAsync(r => r.JobName == claim.JobName, ct);
        if (run is null)
        {
            run = JobRun.Create(claim.JobName);
            db.JobRuns.Add(run);
        }

        if (!run.TryAcquire(claim.Owner, claim.Now, claim.Duration))
        {
            return new JobLeaseDecision(false, run.LeaseOwner);
        }

        try
        {
            await db.SaveChangesAsync(ct);
            return new JobLeaseDecision(true, null);
        }
        catch (Exception ex) when (ex is ConcurrencyConflictException or DbUpdateException)
        {
            // Another instance saved the row between the read and this save, and so holds the lease.
            await using var reader = Open();
            var holder = await reader.JobRuns.AsNoTracking()
                .Where(r => r.JobName == claim.JobName)
                .Select(r => r.LeaseOwner)
                .SingleOrDefaultAsync(ct);
            return new JobLeaseDecision(false, holder);
        }
    }

    /// <inheritdoc/>
    public async Task CompleteAsync(JobLeaseRelease release, CancellationToken ct)
    {
        await using var db = Open();

        var run = await db.JobRuns.SingleOrDefaultAsync(r => r.JobName == release.JobName, ct);

        // No row, or a lease taken over after it expired: there is nothing of ours left to give back.
        if (run is null || !run.Complete(release.Owner, release.Now, release.Succeeded, release.Rest))
        {
            return;
        }

        await db.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<DateTimeOffset?> GetLastSucceededAtAsync(string jobName, CancellationToken ct)
    {
        await using var db = Open();

        return await db.JobRuns.AsNoTracking()
            .Where(r => r.JobName == jobName)
            .Select(r => r.LastSucceededAt)
            .SingleOrDefaultAsync(ct);
    }

    /// <summary>Opens a context of its own on the database the scoped context uses.</summary>
    /// <returns>A new context; the caller disposes it.</returns>
    private AppDbContext Open()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_scoped.Database.GetConnectionString())
            .Options;

        return new AppDbContext(options, _encryption);
    }
}
