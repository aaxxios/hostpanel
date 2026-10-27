using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Orders.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Orders.Commands.CreateOrderPaymentIntent;

/// <summary>
/// Handles <see cref="CreateOrderPaymentIntentCommand"/>: resolves the order's invoice, and
/// registers an intent to charge its total at the card gateway.
/// </summary>
public sealed class CreateOrderPaymentIntentHandler
{
    /// <summary>
    /// Currency every card intent is opened in. Named here so the one place it is decided is
    /// greppable; left unchanged from the previous behaviour, since moving a use case is not the
    /// change that should also reprice it.
    /// </summary>
    private const string IntentCurrency = "usd";

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>The card gateway this backend opens intents at.</summary>
    private readonly ICardPaymentGateway _cardGateway;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="cardGateway">The card gateway this backend opens intents at.</param>
    public CreateOrderPaymentIntentHandler(IHostpanelDbContext context, ICardPaymentGateway cardGateway)
    {
        _context = context;
        _cardGateway = cardGateway;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The order to open a payment intent for.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The gateway's client secret, or <c>ORDER_NOT_FOUND</c> when no such order exists or the
    /// payment token does not match it, <c>ORDER_NOT_INVOICED</c> when the order was never
    /// billed, or <c>INVOICE_NOT_FOUND</c> when its invoice has gone missing.
    /// </returns>
    public async Task<Result<string>> HandleAsync(CreateOrderPaymentIntentCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var found = await _context.Orders.FirstOrDefaultAsync(o => o.Id == cmd.OrderId, ct);
        var payable = found.EnsurePayableWith(cmd.PaymentToken);
        if (payable.IsFailure)
        {
            return Result<string>.FailureCode(payable.ErrorCode!);
        }

        var order = payable.Value!;
        if (order.InvoiceId is null)
        {
            return Result<string>.FailureCode(HostpanelErrorCodes.ORDER_NOT_INVOICED);
        }

        // Transactions loaded: what is charged is what is still due, which earlier payments reduce.
        var invoice = await _context.Invoices
            .Include(i => i.Transactions)
            .FirstOrDefaultAsync(i => i.Id == order.InvoiceId.Value, ct);
        if (invoice is null)
        {
            return Result<string>.FailureCode(HostpanelErrorCodes.INVOICE_NOT_FOUND);
        }

        // Carried so the gateway's own webhook and dashboard name the same rows this database
        // does -- a payment that arrives with no way back to an order is a manual reconciliation.
        var metadata = new Dictionary<string, string>
        {
            ["orderId"] = order.Id.ToString(),
            ["invoiceId"] = invoice.Id.ToString(),
            ["clientId"] = order.ClientId.ToString(),
        };

        // Nothing left to charge: an intent for 0 would only fail at the card gateway.
        if (invoice.AmountDue <= 0m)
        {
            return Result<string>.FailureCode(HostpanelErrorCodes.INVOICE_NOT_PAYABLE);
        }

        // What is still due, not the total: a payment recorded earlier must not be charged again.
        var clientSecret = await _cardGateway.CreatePaymentIntentAsync(invoice.AmountDue, IntentCurrency, metadata, ct);
        return Result.Success(clientSecret);
    }
}
