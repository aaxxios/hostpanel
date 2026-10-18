using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wolverine;
namespace Innovayse.Hostpanel.Application.Billing.Commands.ProcessBillableItemsCron;
/// <summary>
/// Handles <see cref="ProcessBillableItemsCronCommand"/> by invoicing all billable items
/// that are due for automatic processing, including recurring items.
/// Re-schedules itself for the next day after completion.
/// </summary>
public sealed class ProcessBillableItemsCronHandler
{
    /// <summary>Hour (UTC) at which this job runs daily.</summary>
    private const int CronHour = 6;

    /// <summary>Default payment term in days for auto-generated invoices.</summary>
    private const int PaymentTermDays = 14;

    /// <summary>The module's data context, for the billable items due and the invoices created for them.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>The currency each invoiced client is billed in, stamped on their invoice.</summary>
    private readonly IPayerCurrencyResolver _payerCurrency;

    /// <summary>Unit of work.</summary>
    private readonly IUnitOfWork _uow;

    /// <summary>Message bus, for re-scheduling the job.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Structured logger.</summary>
    private readonly ILogger<ProcessBillableItemsCronHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context, for the billable items due and the invoices created for them.</param>
    /// <param name="payerCurrency">The currency each invoiced client is billed in, stamped on their invoice.</param>
    /// <param name="uow">Unit of work.</param>
    /// <param name="bus">Message bus, for re-scheduling the job.</param>
    /// <param name="logger">Structured logger.</param>
    public ProcessBillableItemsCronHandler(
        IHostpanelDbContext context,
        IPayerCurrencyResolver payerCurrency,
        IUnitOfWork uow,
        IMessageBus bus,
        ILogger<ProcessBillableItemsCronHandler> logger)
    {
        _context = context;
        _payerCurrency = payerCurrency;
        _uow = uow;
        _bus = bus;
        _logger = logger;
    }

    /// <summary>
    /// Processes all due billable items and creates invoices grouped by client.
    /// </summary>
    /// <param name="cmd">The scheduled command (marker, no payload).</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task HandleAsync(ProcessBillableItemsCronCommand cmd, CancellationToken ct)
    {
        _ = cmd;

        await ProcessCronItemsAsync(ct);
        await ProcessRecurringItemsAsync(ct);

        // Re-schedule for next day
        var nextRun = DailySchedule.NextOccurrence(CronHour, DateTimeOffset.UtcNow);
        await _bus.ScheduleAsync(new ProcessBillableItemsCronCommand(), nextRun);
        _logger.LogInformation("ProcessBillableItemsCronCommand re-scheduled for {NextRun:u}.", nextRun);
    }

    /// <summary>
    /// Invoices all one-time cron items (InvoiceOnNextCron, InvoiceForDueDate with DueDate &lt;= now).
    /// Groups items by client and creates one invoice per client.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    private async Task ProcessCronItemsAsync(CancellationToken ct)
    {
        // Was IBillableItemRepository.GetDueForCronInvoicingAsync. Tracked, not AsNoTracking: each
        // matching item has MarkInvoiced() called on it below and is saved in this same pass.
        var now = DateTimeOffset.UtcNow;
        var items = await _context.BillableItems
            .Where(x => !x.IsInvoiced && x.Type == BillableItemType.OneTime && x.NextDueDate <= now)
            .ToListAsync(ct);
        if (items.Count == 0)
        {
            return;
        }

        var grouped = items.GroupBy(i => i.ClientId);

        foreach (var group in grouped)
        {
            var clientId = group.Key;
            var currency = (await _payerCurrency.ForClientAsync(clientId, ct)).Code;
            var invoice = Invoice.CreateForBillableItems(
                clientId, DateTimeOffset.UtcNow.AddDays(PaymentTermDays), currency, group);

            _context.Invoices.Add(invoice);
            await _uow.SaveChangesAsync(ct);

            foreach (var item in group)
            {
                item.MarkInvoiced(invoice.Id);
            }

            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Created invoice {InvoiceId} for client {ClientId} with {Count} cron billable items.",
                invoice.Id, clientId, group.Count());
        }
    }

    /// <summary>
    /// Processes recurring billable items whose due date has arrived.
    /// Creates an invoice for each recurring item, increments the invoice count,
    /// advances the due date, and resets for the next cycle.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    private async Task ProcessRecurringItemsAsync(CancellationToken ct)
    {
        // Was IBillableItemRepository.GetDueForRecurrenceAsync. Tracked, not AsNoTracking: each
        // matching item has MarkInvoiced()/AdvanceDueDate() called on it below and is saved in
        // this same pass.
        var now = DateTimeOffset.UtcNow;
        var items = await _context.BillableItems
            .Where(x => x.Type == BillableItemType.Recurring && x.NextDueDate != null && x.NextDueDate <= now)
            .ToListAsync(ct);
        if (items.Count == 0)
        {
            return;
        }

        var grouped = items.GroupBy(i => i.ClientId);

        foreach (var group in grouped)
        {
            var clientId = group.Key;
            var currency = (await _payerCurrency.ForClientAsync(clientId, ct)).Code;
            var invoice = Invoice.CreateForBillableItems(
                clientId, DateTimeOffset.UtcNow.AddDays(PaymentTermDays), currency, group);

            _context.Invoices.Add(invoice);
            await _uow.SaveChangesAsync(ct);

            foreach (var item in group)
            {
                item.MarkInvoiced(invoice.Id);
                item.AdvanceDueDate();
            }

            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Created recurring invoice {InvoiceId} for client {ClientId} with {Count} items.",
                invoice.Id, clientId, group.Count());
        }
    }
}
