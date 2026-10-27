using System.Globalization;
using Innovayse.Hostpanel.Application.Billing.Commands.ApplyPaidRenewals;
using Innovayse.Hostpanel.Application.Billing.Commands.RaiseClientRenewalInvoices;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Services.Commands.UnsuspendService;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Domain.Domains.Enums;
using Innovayse.Hostpanel.Domain.Domains.Events;
using Innovayse.Hostpanel.Domain.Services.Enums;
using Innovayse.Hostpanel.Domain.Settings;
using Innovayse.Hostpanel.Shared.Infrastructure;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wolverine;
namespace Innovayse.Hostpanel.Application.Billing.Commands.ProcessRenewalsCron;

/// <summary>
/// Handles <see cref="ProcessRenewalsCronCommand"/>, the daily renewal run. First any open renewal
/// invoice that owes nothing is marked paid (closing the gap between the two saves of a
/// credit-covered invoice). Then the sweep: every
/// paid invoice with a renewal line that is neither applied nor claimed gets an
/// <see cref="ApplyPaidRenewalsCommand"/> (so a lost payment event is applied within a day); every
/// service suspended for an overdue invoice that no Overdue invoice references any more is
/// unsuspended; staff are told about a registrar claim that has stayed unconfirmed for a day; and
/// one Warning names auto-renewing services that have no renewal date. Then one
/// <see cref="RaiseClientRenewalInvoicesCommand"/> per candidate client, each in its own message
/// scope. Nothing is extended here. Re-schedules itself for the next day, in a finally.
/// </summary>
public sealed class ProcessRenewalsCronHandler
{
    /// <summary>Hour (UTC) at which this job runs daily.</summary>
    private const int CronHour = 5;

    /// <summary>How many service ids the undated-services warning names.</summary>
    private const int UndatedSampleSize = 10;

    /// <summary>The module's data context, for the sweep and the candidate clients.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Message bus, for the per-invoice and per-client commands, staff alerts and re-scheduling.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Structured logger.</summary>
    private readonly ILogger<ProcessRenewalsCronHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context, for the sweep and the candidate clients.</param>
    /// <param name="bus">Message bus, for the per-invoice and per-client commands, staff alerts and re-scheduling.</param>
    /// <param name="logger">Structured logger.</param>
    public ProcessRenewalsCronHandler(
        IHostpanelDbContext context,
        IMessageBus bus,
        ILogger<ProcessRenewalsCronHandler> logger)
    {
        _context = context;
        _bus = bus;
        _logger = logger;
    }

    /// <summary>
    /// Sweeps paid-but-unapplied renewals, lifts stale overdue suspensions, alerts on stale claims,
    /// raises due renewal invoices, and always re-schedules.
    /// </summary>
    /// <param name="cmd">The scheduled command (marker, no payload).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task.</returns>
    public async Task HandleAsync(ProcessRenewalsCronCommand cmd, CancellationToken ct)
    {
        _ = cmd;

        // In a finally so the daily chain survives any failure (a throwing query, a dead database);
        // a missed day is not retried until tomorrow, but the job never silently stops for good.
        try
        {
            await RunAsync(ct);
        }
        finally
        {
            var nextRun = DailySchedule.NextOccurrence(CronHour, DateTimeOffset.UtcNow);
            await _bus.ScheduleAsync(new ProcessRenewalsCronCommand(), nextRun);
            _logger.LogInformation("ProcessRenewalsCronCommand re-scheduled for {NextRun:u}.", nextRun);
        }
    }

    /// <summary>The run itself: heal, sweep, lift, alert, raise. Every step is isolated per item.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task.</returns>
    private async Task RunAsync(CancellationToken ct)
    {
        // Self-heal before the sweep: a credit-covered renewal invoice is saved, then marked paid in
        // a second save; a crash between the two leaves it open with nothing to pay. Paying it here
        // puts its lines in front of the sweep below.
        var healed = await PayZeroTotalRenewalInvoicesAsync(ct);

        // Sweep first: a paid renewal whose PaymentReceivedEvent was lost is applied here, within a
        // day, before anything new is raised. A claimed line (RenewalAttemptAt set) belongs to a
        // registrar call whose outcome a person must confirm; ApplyPaidRenewals would skip it, so
        // an invoice whose only pending lines are claimed is not dispatched at all.
        var unapplied = await _context.Invoices.AsNoTracking()
            .Where(i => i.Status == InvoiceStatus.Paid
                        && i.Items.Any(l => l.RenewalPeriodStart != null && l.RenewalAppliedAt == null && l.RenewalAttemptAt == null))
            .Select(i => i.Id)
            .ToListAsync(ct);
        foreach (var invoiceId in unapplied)
        {
            await DispatchAsync(
                $"ApplyPaidRenewals(invoice {invoiceId})",
                () => _bus.InvokeAsync<Result>(new ApplyPaidRenewalsCommand(invoiceId), ct),
                ct);
        }

        var lifted = await LiftStaleOverdueSuspensionsAsync(ct);

        await AlertStaleClaimsAsync(ct);

        await WarnUndatedServicesAsync(ct);

        // Only the candidate clients are listed here; each RaiseClientRenewalInvoicesCommand applies
        // the full rules (lead time read per run, duplicate guard, pricing) in its own scope.
        // As far ahead as invoice_days_before can reach, so no due client is missed; each
        // per-client command applies the real lead time.
        var horizon = DateTimeOffset.UtcNow.AddDays(BillingSettingKeys.MaxDays);
        var clientIds = await _context.ClientServices.AsNoTracking()
            .Where(s => s.Status == ServiceStatus.Active && s.AutoRenew && s.NextRenewalAt != null && s.NextRenewalAt <= horizon)
            .Select(s => s.ClientId)
            .Union(_context.Domains.AsNoTracking()
                .Where(d => d.Status == DomainStatus.Active && d.AutoRenew && d.ExpiresAt <= horizon)
                .Select(d => d.ClientId))
            .ToListAsync(ct);
        foreach (var clientId in clientIds)
        {
            await DispatchAsync(
                $"RaiseClientRenewalInvoices(client {clientId})",
                () => _bus.InvokeAsync<Result<int>>(new RaiseClientRenewalInvoicesCommand(clientId), ct),
                ct);
        }

        _logger.LogInformation(
            "ProcessRenewalsCron paid {Healed} zero-total invoices, swept {Swept} invoices, lifted {Lifted} suspensions and checked {Clients} clients.",
            healed, unapplied.Count, lifted, clientIds.Count);
    }

    /// <summary>
    /// Unsuspends every service this billing engine suspended (<see cref="SuspensionSource.Overdue"/>)
    /// that no Overdue invoice references any more -- the durable half of "payment lifts an overdue
    /// suspension". The payment path lifts it at once; this catches what that path cannot: a lost
    /// payment event on a legacy (non-renewal) invoice, an unsuspend that failed or was refused, an
    /// overdue invoice an operator cancelled, or a payment that landed while the suspend run was
    /// between its query and its suspension.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>How many services were unsuspended.</returns>
    private async Task<int> LiftStaleOverdueSuspensionsAsync(CancellationToken ct)
    {
        var serviceIds = await _context.ClientServices.AsNoTracking()
            .Where(s => s.Status == ServiceStatus.Suspended && s.SuspensionSource == SuspensionSource.Overdue
                        && !_context.Invoices.Any(i => i.Status == InvoiceStatus.Overdue && i.Items.Any(l => l.ClientServiceId == s.Id)))
            .Select(s => s.Id)
            .ToListAsync(ct);

        var lifted = 0;
        foreach (var serviceId in serviceIds)
        {
            try
            {
                var outcome = await _bus.InvokeAsync<Result>(new UnsuspendServiceCommand(serviceId), ct);
                if (outcome.IsFailure)
                {
                    _logger.LogWarning("Lifting the overdue suspension of service {ServiceId} refused ({Code}); the next run retries it.", serviceId, outcome.ErrorCode);
                }
                else
                {
                    lifted++;
                    _logger.LogInformation("Service {ServiceId} was suspended for an overdue invoice that is no longer overdue; unsuspended.", serviceId);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
            {
                _logger.LogError(ex, "Lifting the overdue suspension of service {ServiceId} failed; the next run retries it.", serviceId);
            }
        }

        return lifted;
    }

    /// <summary>
    /// Logs one Warning for the Active, auto-renewing services with a renewable cycle but no
    /// <c>NextRenewalAt</c>: nothing ever bills them, so they run unpaid with no other signal. One-off
    /// and free cycles legitimately have no renewal date and are not reported.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task.</returns>
    private async Task WarnUndatedServicesAsync(CancellationToken ct)
    {
        var undated = await _context.ClientServices.AsNoTracking()
            .Where(s => s.Status == ServiceStatus.Active && s.AutoRenew && s.NextRenewalAt == null)
            .OrderBy(s => s.Id)
            .Select(s => new { s.Id, s.BillingCycle })
            .ToListAsync(ct);

        var probe = DateTimeOffset.UtcNow;
        var renewable = undated.Where(s => RenewalPeriod.EndOf(probe, s.BillingCycle, probe.Day) is not null).Select(s => s.Id).ToList();
        if (renewable.Count == 0)
        {
            return;
        }

        // Bounded: one line per run, the first ids only, however many there are.
        _logger.LogWarning(
            "{Count} active auto-renewing services have no NextRenewalAt and are never billed; set their renewal date. First ids: {ServiceIds}",
            renewable.Count, string.Join(", ", renewable.Take(UndatedSampleSize)));
    }

    /// <summary>
    /// Marks paid every Unpaid or Overdue invoice that carries a renewal line and owes nothing
    /// (<c>Total == 0</c>), with the credit-balance method and no ledger row, as
    /// <c>RaiseClientRenewalInvoicesHandler</c> would have done had its second save landed.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>How many invoices were paid.</returns>
    private async Task<int> PayZeroTotalRenewalInvoicesAsync(CancellationToken ct)
    {
        var invoices = await _context.Invoices
            .Where(i => (i.Status == InvoiceStatus.Unpaid || i.Status == InvoiceStatus.Overdue)
                        && i.Total == 0
                        && i.Items.Any(l => l.RenewalPeriodStart != null))
            .ToListAsync(ct);

        var paid = 0;
        foreach (var invoice in invoices)
        {
            try
            {
                invoice.MarkPaid(PaymentMethodNames.CreditBalance);
                await _context.SaveChangesAsync(ct);
                paid++;
                _logger.LogWarning("Renewal invoice {InvoiceId} was open with nothing owed; marked paid.", invoice.Id);
            }
            catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
            {
                _logger.LogError(ex, "Could not mark zero-total renewal invoice {InvoiceId} paid; the next run retries it.", invoice.Id);
            }
        }

        return paid;
    }

    /// <summary>
    /// Tells staff about every domain renewal line claimed for a registrar call between one day and
    /// two days plus an hour ago and still not applied: the call's outcome was never recorded (a
    /// timeout, or a save that failed after the registrar renewed) and nothing retries it. The window
    /// is an hour wider than a day so a run that starts a little late (a slow sweep before it, a
    /// restart) cannot let a claim slip between two runs unreported; the price is that a claim in
    /// that hour may be reported on two consecutive runs.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task.</returns>
    private async Task AlertStaleClaimsAsync(CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var from = now.AddDays(-2).AddHours(-1);
        var to = now.AddDays(-1);
        var stale = await _context.Invoices.AsNoTracking()
            .SelectMany(i => i.Items
                .Where(l => l.RegisteredDomainId != null && l.RenewalAppliedAt == null
                            && l.RenewalAttemptAt != null && l.RenewalAttemptAt > from && l.RenewalAttemptAt <= to)
                .Select(l => new { InvoiceId = i.Id, i.ClientId, DomainId = l.RegisteredDomainId!.Value, AttemptAt = l.RenewalAttemptAt!.Value }))
            .ToListAsync(ct);
        if (stale.Count == 0)
        {
            return;
        }

        var domainIds = stale.Select(s => s.DomainId).Distinct().ToList();
        var names = await _context.Domains.AsNoTracking()
            .Where(d => domainIds.Contains(d.Id))
            .ToDictionaryAsync(d => d.Id, d => d.Name, ct);

        foreach (var line in stale)
        {
            var name = names.GetValueOrDefault(line.DomainId, "(deleted)");
            _logger.LogWarning(
                "Renewal of domain {DomainId} (invoice {InvoiceId}) was attempted at {AttemptAt:u} and never confirmed.",
                line.DomainId, line.InvoiceId, line.AttemptAt);
            await _bus.PublishAsync(new DomainRenewalFailedEvent(line.DomainId, line.ClientId, name, line.InvoiceId,
                string.Create(CultureInfo.InvariantCulture,
                    $"Renewal attempted on {line.AttemptAt:yyyy-MM-dd HH:mm} UTC but not confirmed; verify at the registrar.")));
        }
    }

    /// <summary>Dispatches one unit of work and logs, never rethrows, its failure, so one client cannot stop the run.</summary>
    /// <param name="label">What is dispatched, for the log.</param>
    /// <param name="invoke">Invokes the command and returns its result.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task.</returns>
    /// <remarks>
    /// Invoked with the typed <c>InvokeAsync&lt;TResult&gt;</c>: through the untyped overload Wolverine
    /// treats a handler's return value as a cascading message, and a <see cref="Result"/> is not one.
    /// </remarks>
    private async Task DispatchAsync<TResult>(string label, Func<Task<TResult>> invoke, CancellationToken ct)
        where TResult : Result
    {
        try
        {
            var result = await invoke();
            if (result.IsFailure)
            {
                _logger.LogWarning("Renewal step {Step} refused ({Code}); the next run retries it.", label, result.ErrorCode);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            _logger.LogError(ex, "Renewal step {Step} failed; the next run retries it.", label);
        }
    }
}
