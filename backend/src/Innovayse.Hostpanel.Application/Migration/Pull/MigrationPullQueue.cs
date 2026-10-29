using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Commands.RunMigrationPull;
using Innovayse.Hostpanel.Domain.Migration;
using Innovayse.Hostpanel.Shared.Infrastructure;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Migration.Pull;

/// <summary>
/// Hands a migration job to the background queue exactly once. Shared by the first start and by a
/// retry, so both dispatch a pull the same way.
/// </summary>
/// <remarks>
/// The job row carries a concurrency token. Two admins (or two clicks) that both read the job as
/// pending race on the save: the first one queues it, the second one's save conflicts and it
/// dispatches nothing. The message is published only after the save succeeded, so a pull never
/// runs for a job whose queued state was not stored.
/// </remarks>
public sealed class MigrationPullQueue
{
    /// <summary>The module's data context, the unit of work the queued state is saved through.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>The source of the current moment, the first heartbeat of the queued job.</summary>
    private readonly IClock _clock;

    /// <summary>The bus the pull message is published on.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the queue.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="clock">The source of the current moment.</param>
    /// <param name="bus">The bus the pull message is published on.</param>
    public MigrationPullQueue(IHostpanelDbContext context, IClock clock, IMessageBus bus)
    {
        _context = context;
        _clock = clock;
        _bus = bus;
    }

    /// <summary>Queues <paramref name="job"/>, saves it and publishes its pull.</summary>
    /// <param name="job">A pending or failed job, loaded tracked from <see cref="IHostpanelDbContext"/>.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True when this call queued the job; false when a concurrent call queued or changed it first.</returns>
    public async Task<bool> TryQueueAsync(MigrationJob job, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(job);

        job.Queue(_clock.UtcNow);

        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (ConcurrencyConflictException)
        {
            // Someone else changed the job between our read and our write; their request owns it.
            _context.DetachAll();
            return false;
        }

        await _bus.PublishAsync(new RunMigrationPullCommand(job.Id));
        return true;
    }
}
