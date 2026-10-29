using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Migration.Commands.RecoverInterruptedMigrationJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Innovayse.Hostpanel.Infrastructure.Scheduling;

/// <summary>
/// Hosted service that starts the recurring recovery of migration jobs no worker is running any
/// more, so they can be retried instead of showing as running forever.
/// </summary>
/// <remarks>
/// <see cref="RecurringJobs.StartAsync{TCommand}"/> schedules the first pass right now when the job
/// has not succeeded within its interval (the usual case on a start), and at its next regular moment
/// otherwise; the handler schedules every later pass. A job a restart interrupted is failed once its
/// heartbeat is stale, not at once, so a pull another instance is still running is never failed.
/// Runs after the database migrations: <c>Program</c> applies them before the host starts its
/// hosted services. Registered after Wolverine, like the cron startups, so the bus is running.
/// </remarks>
public sealed class MigrationRecoveryStartup : IHostedService
{
    /// <summary>Creates a scope to resolve the recurring jobs runner from.</summary>
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>Creates the hosted service.</summary>
    /// <param name="scopeFactory">Creates a scope to resolve the recurring jobs runner from.</param>
    public MigrationRecoveryStartup(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    /// <summary>Starts the recovery job.</summary>
    /// <param name="cancellationToken">Cancellation token provided by the host.</param>
    /// <returns>A task that completes once the first pass is scheduled.</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var jobs = scope.ServiceProvider.GetRequiredService<RecurringJobs>();

        await jobs.StartAsync<RecoverInterruptedMigrationJobsCommand>(jobs.Schedules.RecoverInterruptedMigrationJobs, cancellationToken);
    }

    /// <summary>No-op: a scheduled run is in memory and the next start catches the job up.</summary>
    /// <param name="cancellationToken">Cancellation token provided by the host.</param>
    /// <returns>A completed task.</returns>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
