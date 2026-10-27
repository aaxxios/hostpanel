using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Audit;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Commands.PayInvoice;

/// <summary>
/// Charges the client via <see cref="IPaymentGateway"/> and marks the invoice as paid.
/// </summary>
public sealed class PayInvoiceHandler
{
    /// <summary>Ledger payment method recorded when the invoice names none.</summary>
    private const string DefaultPaymentMethod = PaymentMethodNames.Card;

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Payment gateway used to charge the client.</summary>
    private readonly IPaymentGateway _gateway;

    /// <summary>Who is calling, for the activity log entry this handler writes.</summary>
    private readonly ICurrentUserService _caller;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="gateway">Payment gateway used to charge the client.</param>
    /// <param name="caller">Who is calling, for the activity log entry this handler writes.</param>
    public PayInvoiceHandler(IHostpanelDbContext context, IPaymentGateway gateway, ICurrentUserService caller)
    {
        _context = context;
        _gateway = gateway;
        _caller = caller;
    }

    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="cmd">The invoice to pay.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>INVOICE_NOT_FOUND</c> when the id names no invoice,
    /// <c>INVOICE_NOT_PAYABLE</c> when it is closed or nothing is left due,
    /// <c>GATEWAY_CHARGE_UPSTREAM_FAILED</c> when the gateway declines the charge, or
    /// <c>GATEWAY_TRANSACTION_MISSING_UPSTREAM_FAILED</c> when it reports success without a
    /// transaction id.
    /// </returns>
    public async Task<Result> HandleAsync(PayInvoiceCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        // Transactions loaded: the card is charged what is still due, which earlier payments reduce.
        var invoice = await _context.Invoices
            .Include(i => i.Transactions)
            .FirstOrDefaultAsync(i => i.Id == cmd.InvoiceId, ct);
        if (invoice is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.INVOICE_NOT_FOUND);
        }

        // Charged in the currency the invoice was issued in. The command used to carry a code
        // the caller chose, defaulting to USD, which charged an AMD invoice's total as dollars.
        // What is still due, not the total: a payment recorded earlier must not be charged again.
        var charged = invoice.AmountDue;
        if (charged <= 0m)
        {
            // Closed, or nothing left due: a charge for 0 would only fail at the card gateway.
            return Result.FailureCode(HostpanelErrorCodes.INVOICE_NOT_PAYABLE);
        }

        var chargeRequest = new ChargeInstruction(invoice.ClientId, invoice.Id, charged, invoice.Currency);
        var result = await _gateway.ChargeAsync(chargeRequest, ct);

        if (!result.Success)
        {
            return Result.FailureCode(HostpanelErrorCodes.GATEWAY_CHARGE_UPSTREAM_FAILED);
        }

        if (string.IsNullOrEmpty(result.TransactionId))
        {
            return Result.FailureCode(HostpanelErrorCodes.GATEWAY_TRANSACTION_MISSING_UPSTREAM_FAILED);
        }

        var method = invoice.PaymentMethod ?? DefaultPaymentMethod;
        invoice.MarkPaid(result.TransactionId, method);

        // Ledger row in the same save as the Paid mark, so "invoice is Paid" and "ledger row exists"
        // are one atomic fact; no event handler writes it.
        if (charged > 0)
        {
            _context.Transactions.Add(Transaction.CreateInvoicePayment(
                invoice,
                DateTimeOffset.UtcNow,
                method,
                result.TransactionId,
                charged,
                0m));
        }

        _context.ActivityLogs.Add(ActivityLog.Create(
            invoice.ClientId,
            $"Invoice Marked Paid - Invoice ID: {invoice.Id}",
            _caller.UserId, _caller.UserName, _caller.Email, _caller.IpAddress));
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
