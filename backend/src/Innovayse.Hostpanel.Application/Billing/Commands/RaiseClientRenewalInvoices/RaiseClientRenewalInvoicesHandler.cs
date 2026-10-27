using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Registrar;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Domain.Domains.Enums;
using Innovayse.Hostpanel.Domain.Domains.Events;
using Innovayse.Hostpanel.Domain.Services.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Billing.Commands.RaiseClientRenewalInvoices;

/// <summary>
/// Handles <see cref="RaiseClientRenewalInvoicesCommand"/>: finds the client's services and domains
/// inside the lead time that are not already invoiced for their next period, prices them, raises one
/// invoice (or one per line under <c>SeparateInvoices</c>), and applies credit. Nothing is extended
/// here -- that happens only when the line is paid (<c>ApplyPaidRenewalsCommand</c>).
/// <para>
/// Due date: the earliest period start on the invoice when that is still ahead; when a period
/// already started (a back-dated renewal date, a service catching up after a missed payment) the
/// client gets the normal payment window instead -- now plus <c>invoice_days_before</c> -- capped at
/// the earliest period on the invoice that has not started yet, so a back-dated line never pushes a
/// future one past its own start.
/// </para>
/// </summary>
/// <remarks>
/// Duplicate rule: any renewal line for the target on a still-open invoice blocks a new one; so does
/// any line on a paid invoice that is not applied yet (its extension is still pending -- a registrar
/// claim left for staff, say -- so an edited date must not bill the same term again); and a line for
/// this exact period blocks it on every invoice except a cancelled one -- so a paid, overdue or
/// refunded period is never re-billed automatically, while a cancelled one is re-raised on the next
/// run while still due (stopping renewal is <c>AutoRenew = false</c>).
/// </remarks>
public sealed class RaiseClientRenewalInvoicesHandler
{
    /// <summary>Statuses of an invoice that is still open, so it blocks a second renewal of the same target.</summary>
    private static readonly InvoiceStatus[] OpenStatuses =
        [InvoiceStatus.Draft, InvoiceStatus.Unpaid, InvoiceStatus.Overdue, InvoiceStatus.Collections, InvoiceStatus.PaymentPending];

    /// <summary>Data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>The client's billing currency.</summary>
    private readonly IPayerCurrencyResolver _payerCurrency;

    /// <summary>Lead time and credit switch.</summary>
    private readonly RenewalSettingsReader _settings;

    /// <summary>Alerts staff about unpriceable domains.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Structured logger.</summary>
    private readonly ILogger<RaiseClientRenewalInvoicesHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">Data context.</param>
    /// <param name="payerCurrency">The client's billing currency.</param>
    /// <param name="settings">Lead time and credit switch.</param>
    /// <param name="bus">Alerts staff about unpriceable domains.</param>
    /// <param name="logger">Structured logger.</param>
    public RaiseClientRenewalInvoicesHandler(
        IHostpanelDbContext context, IPayerCurrencyResolver payerCurrency, RenewalSettingsReader settings,
        IMessageBus bus, ILogger<RaiseClientRenewalInvoicesHandler> logger)
    {
        _context = context;
        _payerCurrency = payerCurrency;
        _settings = settings;
        _bus = bus;
        _logger = logger;
    }

    /// <summary>Raises the client's due renewal invoices.</summary>
    /// <param name="cmd">The client.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The number of invoices raised, or <c>CLIENT_NOT_FOUND</c>.</returns>
    public async Task<Result<int>> HandleAsync(RaiseClientRenewalInvoicesCommand cmd, CancellationToken ct)
    {
        var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == cmd.ClientId, ct);
        if (client is null)
        {
            return Result<int>.FailureCode(HostpanelErrorCodes.CLIENT_NOT_FOUND);
        }

        var settings = await _settings.ReadAsync(ct);
        var now = DateTimeOffset.UtcNow;
        var horizon = now.AddDays(settings.InvoiceDaysBefore);
        var payer = await _payerCurrency.ForClientAsync(client.Id, ct);

        var lines = new List<PendingLine>();
        lines.AddRange(await DueServiceLinesAsync(client.Id, now, horizon, ct));
        lines.AddRange(await DueDomainLinesAsync(client.Id, now, settings, payer, ct));
        if (lines.Count == 0)
        {
            return Result.Success(0);
        }

        var groups = client.SeparateInvoices ? lines.Select(l => new[] { l }) : [lines.ToArray()];
        var raised = 0;
        foreach (var group in groups)
        {
            var invoice = Invoice.Create(client.Id, DueDateOf(group, now, settings.InvoiceDaysBefore), payer.Code);
            foreach (var line in group)
            {
                line.AddTo(invoice);
            }

            if (settings.AutoApplyCredit && client.CreditBalance > 0 && invoice.Total > 0)
            {
                var amount = Math.Min(client.CreditBalance, invoice.Total);
                client.DeductCredit(amount);
                invoice.ApplyCredit(amount);
            }

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync(ct);

            if (invoice.Total == 0)
            {
                // Credit (or a zero-price service) covered it. Marked paid only after the first save:
                // PaymentReceivedEvent is built with the invoice id at MarkPaid time (not deferred),
                // and before the save that id is 0. No ledger row: the money was booked when the
                // credit was added, as PayInvoiceHandler does for a zero total. The payment event's
                // fast path (or the daily sweep) then applies the renewal.
                invoice.MarkPaid(PaymentMethodNames.CreditBalance);
                await _context.SaveChangesAsync(ct);
            }

            raised++;

            _logger.LogInformation(
                "Raised renewal invoice {InvoiceId} for client {ClientId} with {Count} lines.", invoice.Id, client.Id, group.Length);
        }

        return Result.Success(raised);
    }

    /// <summary>
    /// The due date of an invoice carrying <paramref name="group"/>: the earliest period start when it
    /// is still ahead; otherwise now plus the lead time, capped at the earliest start still ahead.
    /// </summary>
    /// <param name="group">The lines on the invoice.</param>
    /// <param name="now">The moment of this run.</param>
    /// <param name="leadDays">The configured <c>invoice_days_before</c>.</param>
    /// <returns>The due date.</returns>
    private static DateTimeOffset DueDateOf(IReadOnlyCollection<PendingLine> group, DateTimeOffset now, int leadDays)
    {
        var earliest = group.Min(l => l.Start);
        if (earliest >= now)
        {
            return earliest;
        }

        // A period that already started would otherwise be born overdue -- and suspended after the
        // grace -- before the client could see the invoice. It gets the normal payment window.
        var window = now.AddDays(leadDays);
        var earliestFuture = group.Where(l => l.Start >= now).Select(l => (DateTimeOffset?)l.Start).Min() ?? DateTimeOffset.MaxValue;
        return window < earliestFuture ? window : earliestFuture;
    }

    /// <summary>The client's services due a renewal line.</summary>
    /// <param name="clientId">The client.</param>
    /// <param name="now">The moment of this run.</param>
    /// <param name="horizon">Now plus the lead time.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The lines to raise.</returns>
    /// <remarks>
    /// A service set to terminate at the end of its cycle, or with an open cancellation request, is
    /// not invoiced. Termination of those is someone else's job; when its renewal date has already
    /// passed it is running unbilled, and a Warning says so on every run until it is terminated.
    /// </remarks>
    private async Task<List<PendingLine>> DueServiceLinesAsync(int clientId, DateTimeOffset now, DateTimeOffset horizon, CancellationToken ct)
    {
        // AutoTerminateEndOfCycle is filtered below rather than here, so a lapsed one can be reported.
        var services = await _context.ClientServices.AsNoTracking()
            .Where(s => s.ClientId == clientId && s.Status == ServiceStatus.Active && s.AutoRenew
                        && s.NextRenewalAt != null && s.NextRenewalAt <= horizon)
            .ToListAsync(ct);
        if (services.Count == 0)
        {
            return [];
        }

        var ids = services.Select(s => s.Id).ToList();
        var cancelling = await _context.CancellationRequests.AsNoTracking()
            .Where(r => ids.Contains(r.ServiceId) && r.Status == CancellationStatus.Open)
            .Select(r => r.ServiceId)
            .ToListAsync(ct);
        var productIds = services.Select(s => s.ProductId).Distinct().ToList();
        var productNames = await _context.Products.AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Name, ct);

        var result = new List<PendingLine>();
        foreach (var service in services)
        {
            var start = service.NextRenewalAt!.Value;
            var terminating = service.AutoTerminateEndOfCycle;
            if (terminating || cancelling.Contains(service.Id))
            {
                if (start < now)
                {
                    _logger.LogWarning(
                        "Service {ServiceId} is running past its paid period without billing (renewal date {NextRenewalAt:u}; {Why}); terminate it or clear the flag.",
                        service.Id, start, terminating ? "set to terminate at the end of its cycle" : "open cancellation request");
                }

                continue;
            }

            var end = RenewalPeriod.EndOf(start, service.BillingCycle, service.CreatedAt.Day);
            if (end is null)
            {
                _logger.LogWarning("Service {ServiceId} has a non-renewable cycle '{Cycle}'; not invoiced.", service.Id, service.BillingCycle);
                continue;
            }

            if (service.RecurringAmount < 0)
            {
                // A bad price on one service must not stop the client's other renewals; staff fix
                // the amount and the next run raises it.
                _logger.LogWarning(
                    "Service {ServiceId} has a negative recurring amount {Amount}; not invoiced.", service.Id, service.RecurringAmount);
                continue;
            }

            if (await IsServiceInvoicedAsync(service.Id, start, ct))
            {
                continue;
            }

            var name = productNames.GetValueOrDefault(service.ProductId, $"Service #{service.Id}");
            result.Add(new PendingLine(start, inv => inv.AddServiceRenewal($"Renewal: {name}", service.RecurringAmount, service.Id, start, end.Value)));
        }

        return result;
    }

    /// <summary>The client's domains due a renewal line, priced from their TLD.</summary>
    /// <param name="clientId">The client.</param>
    /// <param name="now">The moment of this run.</param>
    /// <param name="settings">Lead time and reminder days (the alert checkpoints).</param>
    /// <param name="payer">The currency the invoice is issued in.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The lines to raise.</returns>
    private async Task<List<PendingLine>> DueDomainLinesAsync(
        int clientId, DateTimeOffset now, RenewalSettings settings, Currency payer, CancellationToken ct)
    {
        var horizon = now.AddDays(settings.InvoiceDaysBefore);
        var domains = await _context.Domains.AsNoTracking()
            .Where(d => d.ClientId == clientId && d.Status == DomainStatus.Active && d.AutoRenew && d.ExpiresAt <= horizon)
            .ToListAsync(ct);

        var result = new List<PendingLine>();
        foreach (var domain in domains)
        {
            var start = domain.ExpiresAt;
            if (await IsDomainInvoicedAsync(domain.Id, start, ct))
            {
                continue;
            }

            var years = Math.Max(1, domain.RegistrationPeriod);
            var (price, reason) = await RenewPriceAsync(domain, years, payer, ct);
            if (price is not > 0)
            {
                // Never a zero domain line: a zero invoice is auto-paid, and that would renew at
                // the registrar for free. Staff price the TLD; the next run raises it.
                _logger.LogWarning("Domain {DomainId} ({Name}) not invoiced: {Reason}", domain.Id, domain.Name, reason);

                if (IsAlertCheckpoint(domain.ExpiresAt, now, settings))
                {
                    await _bus.PublishAsync(new DomainRenewalFailedEvent(domain.Id, clientId, domain.Name, 0,
                        $"{reason}; no renewal invoice was raised."));
                }

                continue;
            }

            var end = RenewalPeriod.EndOfYears(start, years);
            result.Add(new PendingLine(start, inv => inv.AddDomainRenewal($"Domain renewal: {domain.Name} ({years}y)", price.Value, domain.Id, start, end)));
        }

        return result;
    }

    /// <summary>
    /// Whether this run mails staff about an unpriceable domain: only on the checkpoint days --
    /// the lead time, each reminder day, and expiry day -- counted as whole days left. Stateless, so
    /// a domain already inside the window at go-live, after a lead-time change, an import or a missed
    /// run still reaches a later checkpoint, while staff get a handful of mails rather than a daily one.
    /// The Warning log is the record on every other run.
    /// </summary>
    /// <param name="expiresAt">The domain's expiry.</param>
    /// <param name="now">The moment of this run.</param>
    /// <param name="settings">Lead time and reminder days.</param>
    /// <returns><see langword="true"/> when the alert is sent this run.</returns>
    private static bool IsAlertCheckpoint(DateTimeOffset expiresAt, DateTimeOffset now, RenewalSettings settings)
    {
        var daysLeft = (int)Math.Ceiling((expiresAt - now).TotalDays);
        return daysLeft == 0 || daysLeft == settings.InvoiceDaysBefore || settings.ReminderDays.Contains(daysLeft);
    }

    /// <summary>
    /// Whether the service already has a renewal line that blocks a new one: any line on a
    /// still-open invoice (so an edited renewal date cannot raise a second bill), any not-yet-applied
    /// line on a paid invoice (same reason: that term is paid, its extension pending), or a line for
    /// this exact period on any invoice except a cancelled one.
    /// </summary>
    /// <param name="serviceId">The service.</param>
    /// <param name="start">The period about to be invoiced.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><see langword="true"/> when a new line must not be raised.</returns>
    private Task<bool> IsServiceInvoicedAsync(int serviceId, DateTimeOffset start, CancellationToken ct) =>
        _context.Invoices.AnyAsync(
            i => i.Status != InvoiceStatus.Cancelled
                 && i.Items.Any(
                     l => l.ClientServiceId == serviceId
                          && l.RenewalPeriodStart != null
                          && (OpenStatuses.Contains(i.Status)
                              || (i.Status == InvoiceStatus.Paid && l.RenewalAppliedAt == null)
                              || l.RenewalPeriodStart == start)),
            ct);

    /// <summary>The domain counterpart of <see cref="IsServiceInvoicedAsync"/>, keyed on <c>(RegisteredDomainId, ExpiresAt)</c>.</summary>
    /// <param name="domainId">The domain.</param>
    /// <param name="start">The period about to be invoiced (the current expiry).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><see langword="true"/> when a new line must not be raised.</returns>
    private Task<bool> IsDomainInvoicedAsync(int domainId, DateTimeOffset start, CancellationToken ct) =>
        _context.Invoices.AnyAsync(
            i => i.Status != InvoiceStatus.Cancelled
                 && i.Items.Any(
                     l => l.RegisteredDomainId == domainId
                          && l.RenewalPeriodStart != null
                          && (OpenStatuses.Contains(i.Status)
                              || (i.Status == InvoiceStatus.Paid && l.RenewalAppliedAt == null)
                              || l.RenewalPeriodStart == start)),
            ct);

    /// <summary>The TLD's renew price for <paramref name="years"/>, in the payer currency.</summary>
    /// <param name="domain">The domain.</param>
    /// <param name="years">The period.</param>
    /// <param name="payer">The invoice currency.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The price, or <see langword="null"/> with the reason staff are told: no TLD configuration or
    /// renew price for the period, or no currency row to convert the sell price with.
    /// </returns>
    private async Task<(decimal? Price, string Reason)> RenewPriceAsync(RegisteredDomain domain, int years, Currency payer, CancellationToken ct)
    {
        var noPrice = $"No renew price for {years} year(s) in the TLD configuration";

        // Longest suffix first, as the registrar resolver matches: "example.co.am" is priced as
        // "co.am" when that TLD is configured, else as "am".
        foreach (var tld in RegistrarProviderResolver.ExtractTldCandidates(domain.Name))
        {
            var config = await _context.TldConfigs.AsNoTracking().FirstOrDefaultAsync(t => t.Tld == tld, ct);
            if (config is null)
            {
                continue;
            }

            if (!config.SellRenew.TryGetValue(years, out var sell))
            {
                return (null, noPrice);
            }

            var from = await _context.Currencies.AsNoTracking().FirstOrDefaultAsync(c => c.Code == config.SellCurrency, ct);
            if (from is null)
            {
                return (null, $"No exchange rate for {config.SellCurrency}\u2192{payer.Code}");
            }

            var price = from.ConvertTo(sell, payer);
            return price > 0 ? (price, string.Empty) : (null, noPrice);
        }

        return (null, noPrice);
    }

    /// <summary>A line waiting to be added: its period start (for the due date) and how to add it.</summary>
    /// <param name="Start">Period start.</param>
    /// <param name="AddTo">Adds the line to an invoice.</param>
    private sealed record PendingLine(DateTimeOffset Start, Action<Invoice> AddTo);
}
