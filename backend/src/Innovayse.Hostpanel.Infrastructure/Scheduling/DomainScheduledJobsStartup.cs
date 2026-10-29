using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Domains.Commands.CheckDomainExpiries;
using Innovayse.Hostpanel.Application.Domains.Commands.SyncPendingDomainStatuses;
using Innovayse.Hostpanel.Application.Domains.Commands.SyncTldCostPrices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Innovayse.Hostpanel.Infrastructure.Scheduling;

/// <summary>
/// Hosted service that starts the domain maintenance jobs on application startup: the expiry check,
/// the pending status sync and the TLD cost price sync. Their hours and intervals are the
/// <c>ScheduledJobsOptions</c> values the job handlers read too.
/// </summary>
/// <remarks>
/// <see cref="RecurringJobs.StartAsync{TCommand}"/> schedules each job for its next regular run, or
/// right now when the job missed its last one. Each handler schedules its own later runs, so the
/// jobs continue on subsequent days without restarting the application.
/// </remarks>
public sealed class DomainScheduledJobsStartup : IHostedService
{
    /// <summary>Opens the scope each startup run resolves its services in.</summary>
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>Initializes a new instance of the <see cref="DomainScheduledJobsStartup"/> class.</summary>
    /// <param name="scopeFactory">Opens the scope each startup run resolves its services in.</param>
    public DomainScheduledJobsStartup(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    /// <summary>Starts every domain maintenance job.</summary>
    /// <param name="cancellationToken">Cancellation token provided by the host.</param>
    /// <returns>A <see cref="Task"/> that completes once the first run of every job is scheduled.</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var jobs = scope.ServiceProvider.GetRequiredService<RecurringJobs>();
        var schedules = jobs.Schedules;

        await jobs.StartAsync<CheckDomainExpiriesCommand>(schedules.CheckDomainExpiries, cancellationToken);
        await jobs.StartAsync<SyncPendingDomainStatusesCommand>(schedules.SyncPendingDomainStatuses, cancellationToken);
        await jobs.StartAsync<SyncTldCostPricesCommand>(schedules.SyncTldCostPrices, cancellationToken);
    }

    /// <summary>
    /// No-op: nothing needs cleanup on shutdown. A run that was scheduled in memory is lost with the
    /// process, and the next start catches the job up from its last recorded success.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token provided by the host.</param>
    /// <returns>A completed <see cref="Task"/>.</returns>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
