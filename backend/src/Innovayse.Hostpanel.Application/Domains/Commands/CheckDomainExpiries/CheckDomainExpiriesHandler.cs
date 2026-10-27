using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Commands.MarkDomainExpired;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Application.Domains.Extensions;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Domain.Domains.Enums;
using Innovayse.Hostpanel.Domain.Domains.Events;
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
    /// <summary>Warning intervals in days before expiry at which reminder emails are sent.</summary>
    private static readonly int[] WarningDays = [30, 14, 7, 1];

    /// <summary>The data context the expiring domains are read from and the reminder is saved through.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Wolverine bus used to dispatch <see cref="MarkDomainExpiredCommand"/> and to raise the expiry-warning event.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Unit of work the reminder is saved through.</summary>
    private readonly IUnitOfWork _uow;

    /// <summary>Structured logger.</summary>
    private readonly ILogger<CheckDomainExpiriesHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The data context the expiring domains are read from and the reminder is saved through.</param>
    /// <param name="bus">Wolverine bus used to dispatch <see cref="MarkDomainExpiredCommand"/> and to raise the expiry-warning event.</param>
    /// <param name="uow">Unit of work the reminder is saved through.</param>
    /// <param name="logger">Structured logger.</param>
    public CheckDomainExpiriesHandler(
        IHostpanelDbContext context, IMessageBus bus, IUnitOfWork uow, ILogger<CheckDomainExpiriesHandler> logger)
    {
        _context = context;
        _bus = bus;
        _uow = uow;
        _logger = logger;
    }

    /// <summary>
    /// Handles <see cref="CheckDomainExpiriesCommand"/>.
    /// Marks expired domains and sends expiry warning notifications for domains approaching expiry.
    /// </summary>
    /// <param name="cmd">The check domain expiries command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task HandleAsync(CheckDomainExpiriesCommand cmd, CancellationToken ct)
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
    /// </summary>
    /// <param name="now">The current UTC time.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SendExpiryWarningsAsync(DateTimeOffset now, CancellationToken ct)
    {
        var maxThreshold = now.AddDays(WarningDays[0] + 1);
        var expiringDomains = await ListExpiringBeforeAsync(maxThreshold, ct);

        foreach (var domain in expiringDomains)
        {
            if (domain.Status != DomainStatus.Active || domain.ExpiresAt <= now
                // Auto-renewing domains get the renewal invoice's own reminders; a second "expiring" mail would contradict them.
                || domain.AutoRenew)
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
}
