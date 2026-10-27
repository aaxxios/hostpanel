using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Commands.MarkDomainExpired;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Application.Domains.Extensions;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Domain.Domains.Enums;
using Innovayse.Hostpanel.Domain.Domains.Events;
using Innovayse.Hostpanel.Shared.Infrastructure;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wolverine;
namespace Innovayse.Hostpanel.Application.Domains.Commands.CheckDomainExpiries;

/// <summary>
/// Scheduled handler that finds all active domains past their expiry date
/// and dispatches <see cref="MarkDomainExpiredCommand"/> for each.
/// Also sends expiry warning notifications at 30, 14, 7, and 1 days before expiry.
/// </summary>
public sealed class CheckDomainExpiriesHandler
{
    /// <summary>Hour (UTC) at which the daily check runs; the startup schedules the first run for it.</summary>
    public const int CronHour = 9;

    /// <summary>Warning intervals in days before expiry at which reminder emails are sent.</summary>
    private static readonly int[] WarningDays = [30, 14, 7, 1];

    /// <summary>The data context the expiring domains are read from and the reminder is saved through.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Wolverine bus used to dispatch <see cref="MarkDomainExpiredCommand"/> and to raise the expiry-warning event.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Unit of work the reminder is saved through.</summary>
    private readonly IUnitOfWork _uow;

    /// <summary>Billing settings: whether renewal reminders are configured at all.</summary>
    private readonly RenewalSettingsReader _settings;

    /// <summary>Structured logger.</summary>
    private readonly ILogger<CheckDomainExpiriesHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The data context the expiring domains are read from and the reminder is saved through.</param>
    /// <param name="bus">Wolverine bus used to dispatch <see cref="MarkDomainExpiredCommand"/> and to raise the expiry-warning event.</param>
    /// <param name="uow">Unit of work the reminder is saved through.</param>
    /// <param name="settings">Billing settings: whether renewal reminders are configured at all.</param>
    /// <param name="logger">Structured logger.</param>
    public CheckDomainExpiriesHandler(
        IHostpanelDbContext context, IMessageBus bus, IUnitOfWork uow, RenewalSettingsReader settings,
        ILogger<CheckDomainExpiriesHandler> logger)
    {
        _context = context;
        _bus = bus;
        _uow = uow;
        _settings = settings;
        _logger = logger;
    }

    /// <summary>
    /// Handles <see cref="CheckDomainExpiriesCommand"/>.
    /// Marks expired domains and sends expiry warning notifications for domains approaching expiry,
    /// then schedules tomorrow's check, whether or not this one succeeded.
    /// </summary>
    /// <param name="cmd">The check domain expiries command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task HandleAsync(CheckDomainExpiriesCommand cmd, CancellationToken ct)
    {
        _ = cmd;

        return RecurringJob.RunAsync(
            nameof(CheckDomainExpiriesCommand), () => RunAsync(ct), ScheduleNextAsync, _logger, ct);
    }

    /// <summary>One pass of the job; <see cref="HandleAsync"/> runs it through <see cref="RecurringJob"/>.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task.</returns>
    private async Task RunAsync(CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;

        // ── Mark already-expired domains ─────────────────────────────────
        var expiredDomains = await ListExpiringBeforeAsync(now, ct);

        foreach (var domain in expiredDomains)
        {
            if (domain.Status == DomainStatus.Active)
            {
                var outcome = await _bus.InvokeAsync<Result>(new MarkDomainExpiredCommand(domain.Id), ct);
                if (outcome.IsFailure)
                {
                    _logger.LogError(
                        "Marking domain {DomainId} expired failed: {ErrorCode}.", domain.Id, outcome.ErrorCode);
                }
            }
        }

        // ── Send expiry warnings ─────────────────────────────────────────
        await SendExpiryWarningsAsync(now, ct);
    }

    /// <summary>
    /// Schedules tomorrow's check. Before this the job never scheduled itself: it ran once after each
    /// start, and domains stopped being marked expired or warned about until the next restart.
    /// </summary>
    /// <returns>A task.</returns>
    private async Task ScheduleNextAsync()
    {
        var nextRun = DailySchedule.NextOccurrence(CronHour, DateTimeOffset.UtcNow);
        await _bus.ScheduleAsync(new CheckDomainExpiriesCommand(), nextRun);
        _logger.LogInformation("CheckDomainExpiriesCommand re-scheduled for {NextRun:u}.", nextRun);
    }

    /// <summary>
    /// Returns every active domain expiring before the given threshold, with the same graph a
    /// reminder write needs.
    /// </summary>
    /// <param name="threshold">Upper-bound expiry date (exclusive).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Domains expiring before the threshold.</returns>
    private async Task<List<RegisteredDomain>> ListExpiringBeforeAsync(
        DateTimeOffset threshold, CancellationToken ct) =>
        await _context.Domains
            .IncludeChildren()
            .Where(x => x.ExpiresAt < threshold && x.Status == DomainStatus.Active)
            .ToListAsync(ct);

    /// <summary>
    /// Finds active domains expiring within the next 30 days and raises
    /// <see cref="DomainExpiringEvent"/> for each matching warning interval,
    /// provided a reminder of that type has not already been sent.
    /// <para>
    /// A domain whose renewal line sits on an open invoice is skipped while renewal reminders are
    /// configured: the client is already being reminded about that invoice, and a second "expiring"
    /// mail would contradict it. Every other domain -- auto-renewing or not -- still gets the warning:
    /// one the renewal run could not price, one whose renewal invoice was cancelled, or any domain
    /// once the reminder days are cleared, would otherwise expire without a word to the client.
    /// </para>
    /// </summary>
    /// <param name="now">The current UTC time.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SendExpiryWarningsAsync(DateTimeOffset now, CancellationToken ct)
    {
        var maxThreshold = now.AddDays(WarningDays[0] + 1);
        var expiringDomains = await ListExpiringBeforeAsync(maxThreshold, ct);
        var beingReminded = await DomainsBeingRemindedAsync(expiringDomains, ct);

        foreach (var domain in expiringDomains)
        {
            if (domain.Status != DomainStatus.Active || domain.ExpiresAt <= now || beingReminded.Contains(domain.Id))
            {
                continue;
            }

            var daysUntilExpiry = (int)Math.Ceiling((domain.ExpiresAt - now).TotalDays);

            foreach (var warningDay in WarningDays)
            {
                if (daysUntilExpiry > warningDay)
                {
                    continue;
                }

                var reminderType = $"{warningDay} Days Before Expiry";

                if (domain.Reminders.Any(r => r.ReminderType == reminderType))
                {
                    continue;
                }

                try
                {
                    await _bus.PublishAsync(
                        new DomainExpiringEvent(domain.Id, domain.ClientId, domain.Name, domain.ExpiresAt));

                    domain.AddReminder(reminderType, $"client-{domain.ClientId}");
                    await _uow.SaveChangesAsync(ct);

                    _logger.LogInformation(
                        "Raised expiry warning ({ReminderType}) for domain {DomainName} (ID {DomainId}).",
                        reminderType,
                        domain.Name,
                        domain.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to send expiry warning ({ReminderType}) for domain {DomainId}.",
                        reminderType,
                        domain.Id);
                }

                break;
            }
        }
    }

    /// <summary>
    /// The ids among <paramref name="domains"/> whose client is already reminded by a renewal invoice:
    /// a domain renewal line on an open invoice, with renewal reminders configured.
    /// </summary>
    /// <param name="domains">The domains about to be warned.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The domain ids to skip; empty when reminders are cleared.</returns>
    private async Task<HashSet<int>> DomainsBeingRemindedAsync(IReadOnlyCollection<RegisteredDomain> domains, CancellationToken ct)
    {
        if (domains.Count == 0)
        {
            return [];
        }

        var settings = await _settings.ReadAsync(ct);
        if (settings.ReminderDays.Count == 0)
        {
            // No renewal reminders go out, so the expiry warning is the only one the client gets.
            return [];
        }

        var ids = domains.Select(d => d.Id).ToList();
        var reminded = await _context.Invoices.AsNoTracking()
            .Where(i => RenewalInvoiceStatuses.Open.Contains(i.Status))
            .SelectMany(i => i.Items)
            .Where(l => l.RegisteredDomainId != null && ids.Contains(l.RegisteredDomainId.Value) && l.RenewalPeriodStart != null)
            .Select(l => l.RegisteredDomainId!.Value)
            .Distinct()
            .ToListAsync(ct);
        return [.. reminded];
    }
}
