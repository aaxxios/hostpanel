using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Billing.Queries.ListDomainsOnOpenRenewalInvoices;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Commands.MarkDomainExpired;
using Innovayse.Hostpanel.Application.Domains.Commands.SendDomainExpiryWarning;
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
    /// <summary>The data context the ids of the lapsed and expiring domains are read from.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Wolverine bus used to dispatch <see cref="MarkDomainExpiredCommand"/> and <see cref="SendDomainExpiryWarningCommand"/> and to ask billing which domains are on open renewal invoices.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Runs the passes under their lease and schedules the next run.</summary>
    private readonly RecurringJobs _jobs;

    /// <summary>Billing settings: whether renewal reminders are configured at all.</summary>
    private readonly RenewalSettingsReader _settings;

    /// <summary>Structured logger.</summary>
    private readonly ILogger<CheckDomainExpiriesHandler> _logger;

    /// <summary>The source of the current moment.</summary>
    private readonly IClock _clock;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The data context the ids of the lapsed and expiring domains are read from.</param>
    /// <param name="bus">Wolverine bus used to dispatch <see cref="MarkDomainExpiredCommand"/> and <see cref="SendDomainExpiryWarningCommand"/> and to ask billing which domains are on open renewal invoices.</param>
    /// <param name="jobs">Runs the passes under their lease and schedules the next run.</param>
    /// <param name="settings">Billing settings: whether renewal reminders are configured at all.</param>
    /// <param name="logger">Structured logger.</param>
    /// <param name="clock">The source of the current moment.</param>
    public CheckDomainExpiriesHandler(
        IHostpanelDbContext context, IMessageBus bus, RecurringJobs jobs, RenewalSettingsReader settings,
        ILogger<CheckDomainExpiriesHandler> logger,
        IClock clock)
    {
        _context = context;
        _bus = bus;
        _jobs = jobs;
        _settings = settings;
        _logger = logger;
        _clock = clock;
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

        return _jobs.RunAsync<CheckDomainExpiriesCommand>(
            _jobs.Schedules.CheckDomainExpiries, () => RunAsync(ct), ct);
    }

    /// <summary>One pass of the job; <see cref="HandleAsync"/> runs it through <see cref="RecurringJobs"/>.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task.</returns>
    private async Task RunAsync(CancellationToken ct)
    {
        var now = _clock.UtcNow;

        // The two phases are independent: marking lapsed domains failing wholesale (a dead query, say)
        // must not cost the client the expiry warnings of the domains still ahead of their date.
        await RunPhaseAsync("marking lapsed domains expired", () => MarkLapsedDomainsAsync(now, ct), ct);
        await RunPhaseAsync("sending expiry warnings", () => SendExpiryWarningsAsync(now, ct), ct);
    }

    /// <summary>Runs one phase of the pass and logs, never rethrows, its failure, so the next phase still runs.</summary>
    /// <param name="phase">What the phase does, for the log.</param>
    /// <param name="run">The phase.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task.</returns>
    private async Task RunPhaseAsync(string phase, Func<Task> run, CancellationToken ct)
    {
        try
        {
            await run();
        }
        catch (Exception ex) when (JobFailure.IsFailure(ex, ct))
        {
            _logger.LogError(
                ex, "{JobName} failed while {Phase}; the next run retries it.", nameof(CheckDomainExpiriesCommand), phase);
        }
    }

    /// <summary>
    /// Sends a <see cref="MarkDomainExpiredCommand"/> for every active domain already past its expiry
    /// date. The ids are read in keyset batches (nothing is tracked here), and each command runs in
    /// its own message scope, so one domain that cannot be marked never stops the others.
    /// </summary>
    /// <param name="now">The moment this pass started.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task.</returns>
    private async Task MarkLapsedDomainsAsync(DateTimeOffset now, CancellationToken ct)
    {
        var lastId = 0;
        while (true)
        {
            var domainIds = await _context.Domains.AsNoTracking()
                .Where(x => x.Id > lastId && x.ExpiresAt < now && x.Status == DomainStatus.Active)
                .OrderBy(x => x.Id)
                .Select(x => x.Id)
                .Take(JobBatch.Size)
                .ToListAsync(ct);
            if (domainIds.Count == 0)
            {
                return;
            }

            foreach (var domainId in domainIds)
            {
                try
                {
                    var outcome = await _bus.InvokeAsync<Result>(new MarkDomainExpiredCommand(domainId), ct);
                    if (outcome.IsFailure)
                    {
                        _logger.LogError(
                            "Marking domain {DomainId} expired failed: {ErrorCode}.", domainId, outcome.ErrorCode);
                    }
                }
                catch (Exception ex) when (JobFailure.IsFailure(ex, ct))
                {
                    _logger.LogError(ex, "Marking domain {DomainId} expired failed; the next run retries it.", domainId);
                }
            }

            lastId = domainIds[^1];
        }
    }

    /// <summary>
    /// Sends a <see cref="SendDomainExpiryWarningCommand"/> for every active domain expiring within the
    /// warning window, which lets the domain pick the warning due and raise
    /// <see cref="DomainExpiringEvent"/> only once per expiry cycle.
    /// <para>
    /// A domain whose renewal line sits on an open invoice is skipped while renewal reminders are
    /// configured: the client is already being reminded about that invoice, and a second "expiring"
    /// mail would contradict it. Every other domain, auto-renewing or not, still gets the warning:
    /// one the renewal run could not price, one whose renewal invoice was cancelled, or any domain
    /// once the reminder days are cleared, would otherwise expire without a word to the client.
    /// </para>
    /// <para>
    /// Each warning is its own command in its own message scope: a warning whose save fails leaves no
    /// tracked change behind that could fail the next domain's save.
    /// </para>
    /// </summary>
    /// <param name="now">The current UTC time.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SendExpiryWarningsAsync(DateTimeOffset now, CancellationToken ct)
    {
        var maxThreshold = now.AddDays(RegisteredDomain.ExpiryWarningWindowDays + 1);
        var lastId = 0;
        while (true)
        {
            // A domain at or past its expiry is never warned (the domain refuses), so only those still ahead are read.
            var domainIds = await _context.Domains.AsNoTracking()
                .Where(x => x.Id > lastId && x.ExpiresAt > now && x.ExpiresAt < maxThreshold && x.Status == DomainStatus.Active)
                .OrderBy(x => x.Id)
                .Select(x => x.Id)
                .Take(JobBatch.Size)
                .ToListAsync(ct);
            if (domainIds.Count == 0)
            {
                return;
            }

            var beingReminded = await DomainsBeingRemindedAsync(domainIds, ct);
            foreach (var domainId in domainIds.Where(id => !beingReminded.Contains(id)))
            {
                try
                {
                    var outcome = await _bus.InvokeAsync<Result>(new SendDomainExpiryWarningCommand(domainId), ct);
                    if (outcome.IsFailure)
                    {
                        _logger.LogWarning(
                            "Expiry warning for domain {DomainId} was refused ({ErrorCode}).", domainId, outcome.ErrorCode);
                    }
                }
                catch (Exception ex) when (JobFailure.IsFailure(ex, ct))
                {
                    _logger.LogWarning(ex, "Failed to send expiry warning for domain {DomainId}.", domainId);
                }
            }

            lastId = domainIds[^1];
        }
    }

    /// <summary>
    /// The ids among <paramref name="domainIds"/> whose client is already reminded by a renewal invoice:
    /// a domain renewal line on an open invoice, with renewal reminders configured.
    /// </summary>
    /// <param name="domainIds">The domains about to be warned.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The domain ids to skip; empty when reminders are cleared.</returns>
    private async Task<HashSet<int>> DomainsBeingRemindedAsync(IReadOnlyCollection<int> domainIds, CancellationToken ct)
    {
        var settings = await _settings.ReadAsync(ct);
        if (settings.ReminderDays.Count == 0)
        {
            // No renewal reminders go out, so the expiry warning is the only one the client gets.
            return [];
        }

        var reminded = await _bus.InvokeAsync<Result<IReadOnlyList<int>>>(
            new ListDomainsOnOpenRenewalInvoicesQuery([.. domainIds]), ct);
        return [.. reminded.Value!];
    }
}
