using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Services;
using Innovayse.Hostpanel.Domain.Services.Enums;
using Innovayse.Hostpanel.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wolverine;
namespace Innovayse.Hostpanel.Application.Billing.Commands.ProcessRenewalsCron;

/// <summary>
/// Handles <see cref="ProcessRenewalsCronCommand"/> by creating renewal invoices for all
/// active services whose next renewal date has arrived.
/// Groups services by client and creates one invoice per client.
/// Re-schedules itself for the next day after completion.
/// </summary>
public sealed class ProcessRenewalsCronHandler
{
    /// <summary>Hour (UTC) at which this job runs daily.</summary>
    private const int CronHour = 5;

    /// <summary>Default payment term in days for renewal invoices.</summary>
    private const int PaymentTermDays = 14;

    /// <summary>The module's data context, for the services due for renewal, products and invoices.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>The currency each renewing client is billed in, stamped on their invoice.</summary>
    private readonly IPayerCurrencyResolver _payerCurrency;

    /// <summary>Unit of work.</summary>
    private readonly IUnitOfWork _uow;

    /// <summary>Message bus, for re-scheduling the job.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Structured logger.</summary>
    private readonly ILogger<ProcessRenewalsCronHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context, for the services due for renewal, products and invoices.</param>
    /// <param name="payerCurrency">The currency each renewing client is billed in, stamped on their invoice.</param>
    /// <param name="uow">Unit of work.</param>
    /// <param name="bus">Message bus, for re-scheduling the job.</param>
    /// <param name="logger">Structured logger.</param>
    public ProcessRenewalsCronHandler(
        IHostpanelDbContext context,
        IPayerCurrencyResolver payerCurrency,
        IUnitOfWork uow,
        IMessageBus bus,
        ILogger<ProcessRenewalsCronHandler> logger)
    {
        _context = context;
        _payerCurrency = payerCurrency;
        _uow = uow;
        _bus = bus;
        _logger = logger;
    }

    /// <summary>
    /// Queries all active services due for renewal, creates invoices grouped by client,
    /// and advances each service's renewal date to the next cycle.
    /// </summary>
    /// <param name="cmd">The scheduled command (marker, no payload).</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task HandleAsync(ProcessRenewalsCronCommand cmd, CancellationToken ct)
    {
        _ = cmd;

        // Was IClientServiceRepository.ListDueForRenewalAsync. Tracked, not AsNoTracking: each
        // matching service has AdvanceRenewal() called on it below and is saved in this same pass.
        var asOf = DateTimeOffset.UtcNow;
        var dueServices = await _context.ClientServices
            .Where(s => s.Status == ServiceStatus.Active && s.NextRenewalAt != null && s.NextRenewalAt <= asOf)
            .ToListAsync(ct);

        if (dueServices.Count > 0)
        {
            var productIds = dueServices.Select(s => s.ProductId).Distinct();

            // Was IProductRepository.FindByIdsAsync -- Include(Prices) kept for parity, though
            // this handler only reads Name; read-only, so no tracking is needed.
            var products = await _context.Products
                .AsNoTracking()
                .Include(p => p.Prices)
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync(ct);
            var productMap = products.ToDictionary(p => p.Id);

            var grouped = dueServices.GroupBy(s => s.ClientId);

            foreach (var group in grouped)
            {
                var clientId = group.Key;
                var currency = (await _payerCurrency.ForClientAsync(clientId, ct)).Code;
                var invoice = Invoice.Create(clientId, DateTimeOffset.UtcNow.AddDays(PaymentTermDays), currency);

                foreach (var service in group)
                {
                    var productName = productMap.TryGetValue(service.ProductId, out var product)
                        ? product.Name
                        : $"Service #{service.Id}";

                    // The service is in hand here, so the line records which one it is for
                    // rather than leaving the customer to read it off the description. This is
                    // the only place in the codebase where an invoice line is raised with the
                    // ClientService it charges for already loaded: the first payment is invoiced
                    // by PlaceOrderHandler, which runs before FulfillPaidOrderHandler creates
                    // the service, and billable-item lines hang off a BillableItem that has no
                    // service of its own.
                    invoice.AddItem(
                        $"Renewal: {productName}", service.RecurringAmount, 1, clientServiceId: service.Id);
                    service.AdvanceRenewal();
                }

                _context.Invoices.Add(invoice);
                await _uow.SaveChangesAsync(ct);

                _logger.LogInformation(
                    "Created renewal invoice {InvoiceId} for client {ClientId} with {Count} services.",
                    invoice.Id, clientId, group.Count());
            }
        }

        _logger.LogInformation(
            "ProcessRenewalsCron processed {Count} services due for renewal.", dueServices.Count);

        // Re-schedule for next day
        var nextRun = DailySchedule.NextOccurrence(CronHour, DateTimeOffset.UtcNow);
        await _bus.ScheduleAsync(new ProcessRenewalsCronCommand(), nextRun);
        _logger.LogInformation("ProcessRenewalsCronCommand re-scheduled for {NextRun:u}.", nextRun);
    }
}
