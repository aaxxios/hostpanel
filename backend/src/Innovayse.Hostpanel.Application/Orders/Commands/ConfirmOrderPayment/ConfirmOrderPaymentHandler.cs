using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Orders.Commands.FulfillPaidOrder;
using Innovayse.Hostpanel.Application.Orders.Extensions;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Orders.Commands.ConfirmOrderPayment;

/// <summary>
/// Confirms a card payment for an order: verifies the PaymentIntent succeeded,
/// marks the linked invoice as paid, and dispatches <see cref="FulfillPaidOrderCommand"/>
/// to accept the order and fulfill its line items.
/// </summary>
public sealed class ConfirmOrderPaymentHandler
{
    /// <summary>Ledger payment method recorded for a confirmed card payment.</summary>
    private const string CardPaymentMethod = PaymentMethodNames.Card;

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Card payment gateway.</summary>
    private readonly ICardPaymentGateway _cardGateway;

    /// <summary>Wolverine message bus used to dispatch order fulfillment.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="cardGateway">Card payment gateway.</param>
    /// <param name="bus">Wolverine message bus used to dispatch order fulfillment.</param>
    public ConfirmOrderPaymentHandler(IHostpanelDbContext context, ICardPaymentGateway cardGateway, IMessageBus bus)
    {
        _context = context;
        _cardGateway = cardGateway;
        _bus = bus;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The order and card gateway payment intent to confirm.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success once the payment is confirmed and fulfillment is dispatched, or
    /// <c>ORDER_NOT_FOUND</c> when no such order exists or the payment token does not match it,
    /// <c>ORDER_NOT_INVOICED</c> when the order was never billed, <c>INVOICE_NOT_FOUND</c> when
    /// its invoice has gone missing, <c>GATEWAY_CHARGE_UPSTREAM_FAILED</c> when the card gateway cannot
    /// verify the PaymentIntent, <c>GATEWAY_TRANSACTION_MISSING_UPSTREAM_FAILED</c> when it
    /// reports success without a transaction id, or whatever fulfillment itself refuses with.
    /// </returns>
    public async Task<Result> HandleAsync(ConfirmOrderPaymentCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var found = await _context.Orders.FirstOrDefaultAsync(o => o.Id == cmd.OrderId, ct);
        var payable = found.EnsurePayableWith(cmd.PaymentToken);
        if (payable.IsFailure)
        {
            return Result.FailureCode(payable.ErrorCode!);
        }

        var order = payable.Value!;
        if (order.InvoiceId is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.ORDER_NOT_INVOICED);
        }

        var (success, transactionId) = await _cardGateway.VerifyPaymentIntentAsync(cmd.PaymentIntentId, ct);
        if (!success)
        {
            return Result.FailureCode(HostpanelErrorCodes.GATEWAY_CHARGE_UPSTREAM_FAILED);
        }

        if (string.IsNullOrEmpty(transactionId))
        {
            return Result.FailureCode(HostpanelErrorCodes.GATEWAY_TRANSACTION_MISSING_UPSTREAM_FAILED);
        }

        var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == order.InvoiceId.Value, ct);
        if (invoice is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.INVOICE_NOT_FOUND);
        }

        invoice.MarkPaid(transactionId);

        // Ledger row in the same save as the Paid mark; no event handler writes it.
        if (invoice.Total > 0)
        {
            _context.Transactions.Add(Transaction.CreateInvoicePayment(
                invoice, DateTimeOffset.UtcNow, CardPaymentMethod, transactionId, invoice.Total, 0m));
        }

        await _context.SaveChangesAsync(ct);

        return await _bus.InvokeAsync<Result>(new FulfillPaidOrderCommand(order.Id), ct);
    }
}
