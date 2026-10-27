using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Services;
using Innovayse.Hostpanel.Domain.Services.Enums;
using Innovayse.Hostpanel.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wolverine;
namespace Innovayse.Hostpanel.Application.Provisioning.Commands.AutoTerminateSuspendedCron;

/// <summary>
/// Handles <see cref="AutoTerminateSuspendedCronCommand"/> by terminating all services
/// that have been suspended longer than <see cref="SuspensionLimitDays"/>.
/// Re-schedules itself for the next day after completion.
/// </summary>
public sealed class AutoTerminateSuspendedCronHandler
{
    /// <summary>Hour (UTC) at which this job runs daily.</summary>
    private const int CronHour = 8;

    /// <summary>Number of days a service can remain suspended before automatic termination.</summary>
    private const int SuspensionLimitDays = 14;

    /// <summary>The module's data context, for the services suspended past the limit.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Unit of work.</summary>
    private readonly IUnitOfWork _uow;

    /// <summary>Message bus, for re-scheduling the job.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Structured logger.</summary>
    private readonly ILogger<AutoTerminateSuspendedCronHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context, for the services suspended past the limit.</param>
    /// <param name="uow">Unit of work.</param>
    /// <param name="bus">Message bus, for re-scheduling the job.</param>
    /// <param name="logger">Structured logger.</param>
    public AutoTerminateSuspendedCronHandler(
        IHostpanelDbContext context,
        IUnitOfWork uow,
        IMessageBus bus,
        ILogger<AutoTerminateSuspendedCronHandler> logger)
    {
        _context = context;
        _uow = uow;
        _bus = bus;
        _logger = logger;
    }

    /// <summary>
    /// Queries all services suspended beyond the allowed limit and terminates them.
    /// Each terminated service raises domain events for provisioning teardown and notifications.
    /// </summary>
    /// <param name="cmd">The scheduled command (marker, no payload).</param>
    /// <param name="ct">Cancellation token.</param>
    public Task HandleAsync(AutoTerminateSuspendedCronCommand cmd, CancellationToken ct)
    {
        _ = cmd;

        return RecurringJob.RunAsync(
            nameof(AutoTerminateSuspendedCronCommand), () => RunAsync(ct), ScheduleNextAsync, _logger, ct);
    }

    /// <summary>One pass of the job; <see cref="HandleAsync"/> runs it through <see cref="RecurringJob"/>.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task.</returns>
    private async Task RunAsync(CancellationToken ct)
    {
        var threshold = DateTimeOffset.UtcNow.AddDays(-SuspensionLimitDays);

        // Was IClientServiceRepository.ListSuspendedBeforeAsync. Tracked, not AsNoTracking: each
        // matching service has Terminate() called on it below and is saved in this same pass.
        var services = await _context.ClientServices
            .Where(s => s.Status == ServiceStatus.Suspended && s.SuspendedAt != null && s.SuspendedAt <= threshold)
            .ToListAsync(ct);

        foreach (var service in services)
        {
            service.Terminate($"Auto-terminated: suspended for over {SuspensionLimitDays} days");
        }

        if (services.Count > 0)
        {
            await _uow.SaveChangesAsync(ct);
        }

        _logger.LogInformation(
            "AutoTerminateSuspendedCron terminated {Count} services suspended over {Days} days.",
            services.Count,
            SuspensionLimitDays);
    }

    /// <summary>Schedules the next pass.</summary>
    /// <returns>A task.</returns>
    private async Task ScheduleNextAsync()
    {
        var nextRun = DailySchedule.NextOccurrence(CronHour, DateTimeOffset.UtcNow);
        await _bus.ScheduleAsync(new AutoTerminateSuspendedCronCommand(), nextRun);
        _logger.LogInformation("AutoTerminateSuspendedCronCommand re-scheduled for {NextRun:u}.", nextRun);
    }
}
