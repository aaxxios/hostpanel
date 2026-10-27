using Innovayse.Hostpanel.Application.Admin.Servers.Commands.SyncServerAccountCountsCron;
using Innovayse.Hostpanel.Application.Billing.Commands.CheckOverdueInvoicesCron;
using Innovayse.Hostpanel.Application.Billing.Commands.ProcessBillableItemsCron;
using Innovayse.Hostpanel.Application.Billing.Commands.ProcessRenewalsCron;
using Innovayse.Hostpanel.Application.Billing.Commands.ReconcileGatewayPaymentsCron;
using Innovayse.Hostpanel.Application.Billing.Commands.SendRenewalRemindersCron;
using Innovayse.Hostpanel.Application.Billing.Commands.SuspendOverdueServicesCron;
using Innovayse.Hostpanel.Application.Provisioning.Commands.AutoTerminateSuspendedCron;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace Innovayse.Hostpanel.Infrastructure.Scheduling;

/// <summary>
/// Hosted service that schedules the daily billing jobs on application startup: renewal invoices (05:00 UTC),
/// billable items processing and renewal reminders (06:00), overdue checks (07:00), overdue suspensions (09:00), plus the server sync,
/// auto-terminate and gateway reconcile jobs.
/// </summary>
public sealed class BillingScheduledJobsStartup : IHostedService
{
    /// <summary>The scope factory this class uses.</summary>
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>The logger this class uses.</summary>
    private readonly ILogger<BillingScheduledJobsStartup> _logger;

    /// <summary>Initializes a new instance of the <see cref="BillingScheduledJobsStartup"/> class.</summary>
    /// <param name="scopeFactory">The scope factory this class uses.</param>
    /// <param name="logger">The logger this class uses.</param>
    public BillingScheduledJobsStartup(IServiceScopeFactory scopeFactory, ILogger<BillingScheduledJobsStartup> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>Hour (UTC) at which <see cref="ProcessBillableItemsCronCommand"/> should execute.</summary>
    private const int BillableItemsCronHour = 6;

    /// <summary>Hour (UTC) at which <see cref="CheckOverdueInvoicesCronCommand"/> should execute.</summary>
    private const int OverdueCronHour = 7;

    /// <summary>Hour (UTC) at which <see cref="ProcessRenewalsCronCommand"/> should execute.</summary>
    private const int RenewalsCronHour = 5;

    /// <summary>Hour (UTC) at which <see cref="SendRenewalRemindersCronCommand"/> should execute.</summary>
    private const int RenewalRemindersCronHour = 6;

    /// <summary>Hour (UTC) at which <see cref="SuspendOverdueServicesCronCommand"/> should execute.</summary>
    private const int SuspendOverdueCronHour = 9;

    /// <summary>Hour (UTC) at which <see cref="AutoTerminateSuspendedCronCommand"/> should execute.</summary>
    private const int AutoTerminateCronHour = 8;

    /// <summary>Hour (UTC) at which <see cref="SyncServerAccountCountsCronCommand"/> should execute.</summary>
    private const int ServerSyncCronHour = 6;

    /// <summary>
    /// Schedules the billable items cron job for its next daily occurrence.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token provided by the host.</param>
    /// <returns>A <see cref="Task"/> that completes once the message is enqueued.</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var billableItemsRun = NextDailyOccurrence(BillableItemsCronHour);
        await bus.ScheduleAsync(new ProcessBillableItemsCronCommand(), billableItemsRun);
        _logger.LogInformation(
            "ProcessBillableItemsCronCommand scheduled for {ScheduledAt:u}.",
            billableItemsRun);

        var overdueRun = NextDailyOccurrence(OverdueCronHour);
        await bus.ScheduleAsync(new CheckOverdueInvoicesCronCommand(), overdueRun);
        _logger.LogInformation(
            "CheckOverdueInvoicesCronCommand scheduled for {ScheduledAt:u}.",
            overdueRun);

        var renewalsRun = NextDailyOccurrence(RenewalsCronHour);
        await bus.ScheduleAsync(new ProcessRenewalsCronCommand(), renewalsRun);
        _logger.LogInformation(
            "ProcessRenewalsCronCommand scheduled for {ScheduledAt:u}.",
            renewalsRun);

        var remindersRun = NextDailyOccurrence(RenewalRemindersCronHour);
        await bus.ScheduleAsync(new SendRenewalRemindersCronCommand(), remindersRun);
        _logger.LogInformation(
            "SendRenewalRemindersCronCommand scheduled for {ScheduledAt:u}.",
            remindersRun);

        var suspendOverdueRun = NextDailyOccurrence(SuspendOverdueCronHour);
        await bus.ScheduleAsync(new SuspendOverdueServicesCronCommand(), suspendOverdueRun);
        _logger.LogInformation(
            "SuspendOverdueServicesCronCommand scheduled for {ScheduledAt:u}.",
            suspendOverdueRun);

        var serverSyncRun = NextDailyOccurrence(ServerSyncCronHour);
        await bus.ScheduleAsync(new SyncServerAccountCountsCronCommand(), serverSyncRun);
        _logger.LogInformation(
            "SyncServerAccountCountsCronCommand scheduled for {ScheduledAt:u}.",
            serverSyncRun);

        var autoTerminateRun = NextDailyOccurrence(AutoTerminateCronHour);
        await bus.ScheduleAsync(new AutoTerminateSuspendedCronCommand(), autoTerminateRun);
        _logger.LogInformation(
            "AutoTerminateSuspendedCronCommand scheduled for {ScheduledAt:u}.",
            autoTerminateRun);

        var reconcileRun = DateTimeOffset.UtcNow.AddMinutes(
            Innovayse.Hostpanel.Application.Billing.Commands.ReconcileGatewayPaymentsCron
                .ReconcileGatewayPaymentsCronHandler.IntervalMinutes);
        await bus.ScheduleAsync(new ReconcileGatewayPaymentsCronCommand(), reconcileRun);
        _logger.LogInformation(
            "ReconcileGatewayPaymentsCronCommand scheduled for {ScheduledAt:u}.", reconcileRun);
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
