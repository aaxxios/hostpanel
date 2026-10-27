using Innovayse.Hostpanel.Application.Domains.Commands.CheckDomainExpiries;
using Innovayse.Hostpanel.Application.Domains.Commands.SyncPendingDomainStatuses;
using Innovayse.Hostpanel.Application.Domains.Commands.SyncTldCostPrices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace Innovayse.Hostpanel.Infrastructure.Scheduling;

/// <summary>
/// Hosted service that schedules the daily domain maintenance jobs on application startup.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="CheckDomainExpiriesCommand"/> fires daily at 09:00 UTC.<br/>
/// <see cref="SyncPendingDomainStatusesCommand"/> fires every 15 minutes starting immediately.<br/>
/// <see cref="SyncTldCostPricesCommand"/> fires daily at 04:00 UTC.
/// </para>
/// <para>
/// Each handler is responsible for re-scheduling itself at the end of its run so that
/// the jobs continue on subsequent days without restarting the application.
/// </para>
/// </remarks>
public sealed class DomainScheduledJobsStartup : IHostedService
{
    /// <summary>The scope factory this class uses.</summary>
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>The logger this class uses.</summary>
    private readonly ILogger<DomainScheduledJobsStartup> _logger;

    /// <summary>Initializes a new instance of the <see cref="DomainScheduledJobsStartup"/> class.</summary>
    /// <param name="scopeFactory">The scope factory this class uses.</param>
    /// <param name="logger">The logger this class uses.</param>
    public DomainScheduledJobsStartup(IServiceScopeFactory scopeFactory, ILogger<DomainScheduledJobsStartup> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>Hour (UTC) at which <see cref="CheckDomainExpiriesCommand"/> should execute.</summary>
    private const int ExpiryCheckHour = 9;

    /// <summary>Interval in minutes between <see cref="SyncPendingDomainStatusesCommand"/> runs.</summary>
    private const int SyncPendingIntervalMinutes = 15;

    /// <summary>Hour (UTC) at which <see cref="SyncTldCostPricesCommand"/> should execute.</summary>
    private const int TldSyncHour = 4;

    /// <summary>
    /// Schedules all domain maintenance jobs.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token provided by the host.</param>
    /// <returns>A <see cref="Task"/> that completes once all messages are enqueued.</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var expiryCheckTime = NextDailyOccurrence(ExpiryCheckHour);
        var syncPendingTime = DateTimeOffset.UtcNow.AddMinutes(SyncPendingIntervalMinutes);

        await using var scope = _scopeFactory.CreateAsyncScope();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        await bus.ScheduleAsync(new CheckDomainExpiriesCommand(), expiryCheckTime);
        _logger.LogInformation(
            "CheckDomainExpiriesCommand scheduled for {ScheduledAt:u}.",
            expiryCheckTime);

        await bus.ScheduleAsync(new SyncPendingDomainStatusesCommand(), syncPendingTime);
        _logger.LogInformation(
            "SyncPendingDomainStatusesCommand scheduled for {ScheduledAt:u} (every {Interval} min).",
            syncPendingTime, SyncPendingIntervalMinutes);

        var tldSyncTime = NextDailyOccurrence(TldSyncHour);
        await bus.ScheduleAsync(new SyncTldCostPricesCommand(), tldSyncTime);
        _logger.LogInformation(
            "SyncTldCostPricesCommand scheduled for {ScheduledAt:u}.",
            tldSyncTime);
    }

    /// <summary>
    /// No-op — scheduled messages persist in the outbox; no cleanup is required on shutdown.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token provided by the host.</param>
    /// <returns>A completed <see cref="Task"/>.</returns>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>
    /// Calculates the next UTC <see cref="DateTimeOffset"/> at which the given hour occurs.
    /// If the hour has already passed today, returns the same hour tomorrow.
    /// </summary>
    /// <param name="hour">Target hour in UTC (0–23).</param>
    /// <returns>The next occurrence of the specified hour in UTC.</returns>
    private static DateTimeOffset NextDailyOccurrence(int hour)
    {
        var now = DateTimeOffset.UtcNow;
        var candidate = new DateTimeOffset(now.Year, now.Month, now.Day, hour, 0, 0, TimeSpan.Zero);

        return candidate > now ? candidate : candidate.AddDays(1);
    }
}
