using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Orders.Commands.FulfillPaidOrder;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.SDK.Plugins;
using Innovayse.SDK.Plugins.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wolverine;
using Innovayse.Hostpanel.Application.Billing.Commands.CompleteGatewayPayment.Enums;
namespace Innovayse.Hostpanel.Application.Billing.Commands.CompleteGatewayPayment;

/// <summary>
/// Handles <see cref="CompleteGatewayPaymentCommand"/>. Called from the payer's
/// return page and from the reconciliation job — both paths converge here, so
/// idempotency is load-bearing.
/// </summary>
public sealed class CompleteGatewayPaymentHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Resolves the configured payment plugin for a module id.</summary>
    private readonly IPaymentPluginResolver _pluginResolver;

    /// <summary>Wolverine message bus, used to trigger the linked order's fulfillment.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Structured logger.</summary>
    private readonly ILogger<CompleteGatewayPaymentHandler> _logger;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="pluginResolver">Resolves the configured payment plugin for a module id.</param>
    /// <param name="bus">Wolverine message bus.</param>
    /// <param name="logger">Structured logger.</param>
    public CompleteGatewayPaymentHandler(
        IHostpanelDbContext context,
        IPaymentPluginResolver pluginResolver,
        IMessageBus bus,
        ILogger<CompleteGatewayPaymentHandler> logger)
    {
        _context = context;
        _pluginResolver = pluginResolver;
        _bus = bus;
        _logger = logger;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The complete command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The completion state, or <c>INVOICE_NOT_FOUND</c>, <c>GATEWAY_SESSION_NOT_FOUND</c> or
    /// <c>PAYMENT_PLUGIN_NOT_CONFIGURED</c>.
    /// </returns>
    public async Task<Result<GatewayCompletionState>> HandleAsync(
        CompleteGatewayPaymentCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var invoice = await _context.Invoices
            .Include(i => i.Transactions)
            .FirstOrDefaultAsync(i => i.Id == cmd.InvoiceId, ct);
        if (invoice is null)
        {
            return Result<GatewayCompletionState>.FailureCode(HostpanelErrorCodes.INVOICE_NOT_FOUND);
        }

        if (invoice.Status is InvoiceStatus.Paid or InvoiceStatus.Refunded)
        {
            return Result<GatewayCompletionState>.Success(GatewayCompletionState.Paid);
        }

        if (invoice.GatewayModule is null || invoice.GatewayOrderId is null)
        {
            return Result<GatewayCompletionState>.FailureCode(HostpanelErrorCodes.GATEWAY_SESSION_NOT_FOUND);
        }

        var plugin = await _pluginResolver.ResolveAsync(invoice.GatewayModule, ct);
        if (plugin is null)
        {
            return Result<GatewayCompletionState>.FailureCode(HostpanelErrorCodes.PAYMENT_PLUGIN_NOT_CONFIGURED);
        }

        var status = await plugin.GetStatusAsync(invoice.GatewayOrderId, ct);

        if (status.State != GatewayPaymentState.Paid)
        {
            _logger.LogInformation(
                "Invoice {InvoiceId} gateway session {GatewayOrderId} is {State} ({Raw}).",
                invoice.Id, invoice.GatewayOrderId, status.State, status.RawStatus);
            return Result<GatewayCompletionState>.Success(
                status.State == GatewayPaymentState.Pending
                    ? GatewayCompletionState.Pending
                    : GatewayCompletionState.Declined);
        }

        var gatewayTransactionId = status.TransactionId ?? invoice.GatewayOrderId;

        // Read before the Paid mark records it: the session charged what was due, not the total.
        var charged = invoice.AmountDue;
        invoice.MarkPaidViaGateway(gatewayTransactionId);

        // The client's ledger row is written here, in the same save as the Paid mark, rather than by
        // an event handler: the concurrent-completion guard below then makes "ledger row exists"
        // and "invoice is Paid" one atomic fact, so the row can neither be missed nor doubled.
        if (charged > 0)
        {
            _context.Transactions.Add(Transaction.CreateInvoicePayment(
                invoice, DateTimeOffset.UtcNow, invoice.GatewayModule, gatewayTransactionId, charged, 0m));
        }

        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Another writer (the result-page auto-check racing the reconciler, or a retried
            // click) already completed this same payment between our read and our write. A
            // fresh AsNoTracking read bypasses this context's identity map, which still holds
            // this invoice as Modified with the in-memory Status = Paid set above, so it goes
            // to the database rather than returning that stale tracked instance.
            var current = await _context.Invoices
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == cmd.InvoiceId, ct);
            if (current?.Status is InvoiceStatus.Paid or InvoiceStatus.Refunded)
            {
                _logger.LogInformation(
                    "Invoice {InvoiceId} was already marked paid by a concurrent completion.", cmd.InvoiceId);
                return Result<GatewayCompletionState>.Success(GatewayCompletionState.Paid);
            }

            throw;
        }

        var order = await _context.Orders.FirstOrDefaultAsync(o => o.InvoiceId == invoice.Id, ct);
        if (order is not null)
        {
            await _bus.InvokeAsync(new FulfillPaidOrderCommand(order.Id), ct);
        }

        return Result<GatewayCompletionState>.Success(GatewayCompletionState.Paid);
    }
}
