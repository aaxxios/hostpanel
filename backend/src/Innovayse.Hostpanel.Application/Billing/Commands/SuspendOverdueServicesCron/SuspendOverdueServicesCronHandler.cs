using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Services.Commands.SuspendService;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Domain.Services.Enums;
using Innovayse.Hostpanel.Shared.Infrastructure;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Billing.Commands.SuspendOverdueServicesCron;

/// <summary>
/// Handles <see cref="SuspendOverdueServicesCronCommand"/>, the daily overdue run: suspends, with
/// <see cref="SuspensionSource.Overdue"/>, only the Active services referenced by lines of invoices that
/// stayed Overdue past <c>billing.renewal.suspend_after_overdue_days</c>. Services that opted out
/// (<c>OverrideAutoSuspend</c>) or are held by a future <c>SuspendUntil</c> are skipped. Coverage by an
/// overdue invoice is re-checked right before each suspension. Each suspension is its own
/// <see cref="SuspendServiceCommand"/> so one failure cannot stop the rest. Re-schedules itself for the next day.
/// </summary>
public sealed class SuspendOverdueServicesCronHandler
{
    /// <summary>Hour (UTC) at which this job runs daily -- after CheckOverdueInvoicesCron (07:00) has marked today's overdue invoices.</summary>
    private const int CronHour = 9;

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Reads the grace period.</summary>
    private readonly RenewalSettingsReader _settings;

    /// <summary>Message bus, for the suspensions and re-scheduling.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Structured logger.</summary>
    private readonly ILogger<SuspendOverdueServicesCronHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="settings">Reads the grace period.</param>
    /// <param name="bus">Message bus, for the suspensions and re-scheduling.</param>
    /// <param name="logger">Structured logger.</param>
    public SuspendOverdueServicesCronHandler(
        IHostpanelDbContext context,
        RenewalSettingsReader settings,
        IMessageBus bus,
        ILogger<SuspendOverdueServicesCronHandler> logger)
    {
        _context = context;
        _settings = settings;
        _bus = bus;
        _logger = logger;
    }

    /// <summary>Suspends services on invoices overdue past the grace period, then re-schedules itself.</summary>
    /// <param name="cmd">Marker.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task.</returns>
    public async Task HandleAsync(SuspendOverdueServicesCronCommand cmd, CancellationToken ct)
    {
        _ = cmd;

        // In a finally so the daily chain survives any failure; a missed day is not retried until tomorrow.
        try
        {
            var settings = await _settings.ReadAsync(ct);
            var now = DateTimeOffset.UtcNow;
            var cutoff = now.AddDays(-settings.SuspendAfterOverdueDays);

            var serviceIds = await _context.Invoices.AsNoTracking()
                .Where(i => i.Status == InvoiceStatus.Overdue && i.DueDate < cutoff)
                .SelectMany(i => i.Items)
                .Where(l => l.ClientServiceId != null)
                .Select(l => l.ClientServiceId!.Value)
                .Distinct()
                .ToListAsync(ct);

            var eligible = await _context.ClientServices.AsNoTracking()
                .Where(s => serviceIds.Contains(s.Id) && s.Status == ServiceStatus.Active && !s.OverrideAutoSuspend
                            && (s.SuspendUntil == null || s.SuspendUntil <= now))
                .OrderBy(s => s.Id)
                .Select(s => s.Id)
                .ToListAsync(ct);

            var suspended = 0;
            foreach (var id in eligible)
            {
                try
                {
                    // Re-checked right before each suspension: the list above can be minutes old by
                    // now, and a payment that landed meanwhile (its unsuspend already ran and found
                    // nothing to lift) must not be answered with a suspension nobody would lift
                    // until the next renewal run.
                    var stillCovered = await _context.Invoices.AsNoTracking()
                        .AnyAsync(i => i.Status == InvoiceStatus.Overdue && i.DueDate < cutoff
                                       && i.Items.Any(l => l.ClientServiceId == id), ct);
                    if (!stillCovered)
                    {
                        _logger.LogInformation("Service {ServiceId} is no longer on an invoice overdue past the grace period; not suspended.", id);
                        continue;
                    }

                    var outcome = await _bus.InvokeAsync<Result>(new SuspendServiceCommand(id, SuspensionSource.Overdue), ct);
                    if (outcome.IsFailure)
                    {
                        _logger.LogWarning("Overdue suspension refused for service {ServiceId} ({Code}).", id, outcome.ErrorCode);
                    }
                    else
                    {
                        suspended++;
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Overdue suspension failed for service {ServiceId}; the next run retries it.", id);
                }
            }

            _logger.LogInformation("SuspendOverdueServicesCron suspended {Count} of {Eligible} eligible services.", suspended, eligible.Count);
        }
        finally
        {
            var nextRun = DailySchedule.NextOccurrence(CronHour, DateTimeOffset.UtcNow);
            await _bus.ScheduleAsync(new SuspendOverdueServicesCronCommand(), nextRun);
            _logger.LogInformation("SuspendOverdueServicesCronCommand re-scheduled for {NextRun:u}.", nextRun);
        }
    }
}
