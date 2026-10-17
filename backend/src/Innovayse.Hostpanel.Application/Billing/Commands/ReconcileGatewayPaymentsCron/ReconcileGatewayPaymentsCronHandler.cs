using Innovayse.Hostpanel.Application.Billing.Commands.CompleteGatewayPayment;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wolverine;
using Innovayse.Hostpanel.Application.Billing.Commands.CompleteGatewayPayment.Enums;
namespace Innovayse.Hostpanel.Application.Billing.Commands.ReconcileGatewayPaymentsCron;
/// <summary>
/// Handles <see cref="ReconcileGatewayPaymentsCronCommand"/>: verifies every unpaid
/// invoice whose gateway session started between 25 minutes and 24 hours ago
/// (younger sessions may still be on the payment page — Hosted gateway sessions last
/// 20 minutes; older ones are abandoned and left for audit), then reschedules itself.
/// </summary>
public sealed class ReconcileGatewayPaymentsCronHandler
{
    /// <summary>Minutes between runs.</summary>
    public const int IntervalMinutes = 5;

    /// <summary>
    /// Minutes a gateway payment session must have been running before it is checked.
    /// Hosted gateway sessions last 20 minutes, so anything younger may still be on the
    /// payment page; 25 minutes leaves a safety margin past that.
    /// </summary>
    private const int SessionSettleMinutes = 25;

    /// <summary>
    /// Hours after which an unpaid gateway session is treated as abandoned and left
    /// out of reconciliation for audit rather than checked forever.
    /// </summary>
    private const int AbandonedAfterHours = 24;

    /// <summary>The module's data context, for the invoices with a pending gateway session.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Wolverine message bus.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Structured logger.</summary>
    private readonly ILogger<ReconcileGatewayPaymentsCronHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context, for the invoices with a pending gateway session.</param>
    /// <param name="bus">Wolverine message bus.</param>
    /// <param name="logger">Structured logger.</param>
    public ReconcileGatewayPaymentsCronHandler(
        IHostpanelDbContext context,
        IMessageBus bus,
        ILogger<ReconcileGatewayPaymentsCronHandler> logger)
    {
        _context = context;
        _bus = bus;
        _logger = logger;
    }

    /// <summary>Handles the cron command.</summary>
    /// <param name="cmd">The cron command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes when all pending sessions were checked.</returns>
    public async Task HandleAsync(ReconcileGatewayPaymentsCronCommand cmd, CancellationToken ct)
    {
        _ = cmd;

        var now = DateTimeOffset.UtcNow;

        try
        {
            var startedAfter = now.AddHours(-AbandonedAfterHours);
            var startedBefore = now.AddMinutes(-SessionSettleMinutes);

            // Was IInvoiceRepository.ListPendingGatewayPaymentsAsync -- read-only here; the
            // dispatched command below reloads and mutates the invoice in its own scope.
            var pending = await _context.Invoices
                .AsNoTracking()
                .Where(i => (i.Status == InvoiceStatus.Unpaid || i.Status == InvoiceStatus.Overdue)
                    && i.GatewayOrderId != null
                    && i.GatewayStartedAt > startedAfter
                    && i.GatewayStartedAt < startedBefore)
                .ToListAsync(ct);

            _logger.LogInformation("Gateway payment reconciliation: {Count} pending session(s).", pending.Count);

            foreach (var invoice in pending)
            {
                try
                {
                    var result = await _bus.InvokeAsync<Result<GatewayCompletionState>>(
                        new CompleteGatewayPaymentCommand(invoice.Id), ct);
                    if (result.IsFailure)
                    {
                        // One bad session must not starve the rest; it will be retried next run.
                        // Logged rather than thrown: the failure already arrived as a Result, and
                        // wrapping a code back into an exception just to reach the catch below
                        // would be exception-as-control-flow for no gain.
                        _logger.LogError(
                            "Reconciliation failed for invoice {InvoiceId}: {Code}.", invoice.Id, result.ErrorCode);
                        continue;
                    }

                    if (result.Value == GatewayCompletionState.Paid)
                    {
                        _logger.LogWarning(
                            "Reconciler recovered a paid-but-unreturned payment for invoice {InvoiceId}.", invoice.Id);
                    }
                }
                catch (Exception ex)
                {
                    // One bad session must not starve the rest; it will be retried next run.
                    _logger.LogError(ex, "Reconciliation failed for invoice {InvoiceId}.", invoice.Id);
                }
            }
        }
        catch (Exception ex)
        {
            // The reconciler is the only safety net for the whole no-webhook payment design —
            // if the query itself throws (DB blip, etc.), the reschedule below in `finally`
            // must still run, or every unpaid gateway session past this point silently stops
            // being checked until the next API restart.
            _logger.LogError(ex, "Gateway payment reconciliation run failed.");
        }
        finally
        {
            await _bus.ScheduleAsync(new ReconcileGatewayPaymentsCronCommand(), now.AddMinutes(IntervalMinutes));
        }
    }
}
