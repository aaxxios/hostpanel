using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Billing.Commands.SendRenewalReminder;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Shared.Infrastructure;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Billing.Commands.SendRenewalRemindersCron;

/// <summary>
/// Handles <see cref="SendRenewalRemindersCronCommand"/>, the daily reminder run: every unpaid
/// renewal invoice that has reached one of the configured thresholds (<c>billing.renewal.reminder_days</c>)
/// gets one reminder mail per threshold, recorded in <c>InvoiceReminders</c> so it is never repeated.
/// Each invoice is handled by its own <see cref="SendRenewalReminderCommand"/>. Re-schedules itself for the next day.
/// </summary>
public sealed class SendRenewalRemindersCronHandler
{
    /// <summary>Hour (UTC) at which this job runs daily.</summary>
    private const int CronHour = 6;

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Reads the reminder thresholds.</summary>
    private readonly RenewalSettingsReader _settings;

    /// <summary>Message bus, for the mails and re-scheduling.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Structured logger.</summary>
    private readonly ILogger<SendRenewalRemindersCronHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="settings">Reads the reminder thresholds.</param>
    /// <param name="bus">Message bus, for the mails and re-scheduling.</param>
    /// <param name="logger">Structured logger.</param>
    public SendRenewalRemindersCronHandler(
        IHostpanelDbContext context,
        RenewalSettingsReader settings,
        IMessageBus bus,
        ILogger<SendRenewalRemindersCronHandler> logger)
    {
        _context = context;
        _settings = settings;
        _bus = bus;
        _logger = logger;
    }

    /// <summary>Dispatches today's reminders, one message per invoice, and re-schedules itself.</summary>
    /// <param name="cmd">Marker.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task.</returns>
    public async Task HandleAsync(SendRenewalRemindersCronCommand cmd, CancellationToken ct)
    {
        _ = cmd;

        // In a finally so the daily chain survives any failure; a missed day is not retried until tomorrow.
        try
        {
            var settings = await _settings.ReadAsync(ct);
            if (settings.ReminderDays.Count > 0)
            {
                var horizon = DateTimeOffset.UtcNow.AddDays(settings.ReminderDays.Max() + 1);
                var invoiceIds = await _context.Invoices.AsNoTracking()
                    .Where(i => i.Status == InvoiceStatus.Unpaid
                                && i.DueDate <= horizon
                                && i.Items.Any(l => l.RenewalPeriodStart != null))
                    .Select(i => i.Id)
                    .ToListAsync(ct);

                // One message (own scope and DbContext) per invoice: one failure cannot poison the rest.
                foreach (var id in invoiceIds)
                {
                    try
                    {
                        var result = await _bus.InvokeAsync<Result>(new SendRenewalReminderCommand(id), ct);
                        if (result.IsFailure)
                        {
                            _logger.LogWarning("Renewal reminder for invoice {InvoiceId} refused ({Code}); the next run retries it.", id, result.ErrorCode);
                        }
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger.LogError(ex, "Renewal reminder for invoice {InvoiceId} failed; the next run retries it.", id);
                    }
                }

                _logger.LogInformation("SendRenewalRemindersCron checked {Invoices} open renewal invoices.", invoiceIds.Count);
            }
        }
        finally
        {
            var nextRun = DailySchedule.NextOccurrence(CronHour, DateTimeOffset.UtcNow);
            await _bus.ScheduleAsync(new SendRenewalRemindersCronCommand(), nextRun);
            _logger.LogInformation("SendRenewalRemindersCronCommand re-scheduled for {NextRun:u}.", nextRun);
        }
    }
}
