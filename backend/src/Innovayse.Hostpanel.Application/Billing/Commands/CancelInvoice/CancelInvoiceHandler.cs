using Innovayse.Hostpanel.Application.Audit.Common;
using Innovayse.Hostpanel.Application.Billing.Extensions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Commands.CancelInvoice;

/// <summary>Cancels an invoice so it will not be collected.</summary>
public sealed class CancelInvoiceHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Records the activity log line for this action, saved with the change it describes.</summary>
    private readonly IActivityLogger _activity;

    /// <summary>Returns the invoice's credit to the client's balance in the same save.</summary>
    private readonly IClientCredit _clientCredit;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="activity">Records the activity log line for this action, saved with the change it describes.</param>
    /// <param name="clientCredit">Returns the invoice's credit to the client's balance in the same save.</param>
    public CancelInvoiceHandler(IHostpanelDbContext context, IActivityLogger activity, IClientCredit clientCredit)
    {
        _context = context;
        _activity = activity;
        _clientCredit = clientCredit;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The cancel command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>INVOICE_NOT_FOUND</c> when the id names no invoice, or
    /// <c>INVOICE_CANCEL_NOT_ALLOWED</c> when it is paid, refunded or already cancelled, or
    /// <c>INVOICE_HAS_PAYMENTS</c> when it already took a payment, or
    /// <c>CLIENT_NOT_FOUND</c> when its credit has no client to go back to. Credit applied to the
    /// invoice goes back to the client's balance.
    /// </returns>
    public async Task<Result> HandleAsync(CancelInvoiceCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        // Payments are loaded because an invoice that already took money may not be cancelled.
        var invoice = await _context.Invoices
            .Include(x => x.Transactions)
            .FirstOrDefaultAsync(x => x.Id == cmd.InvoiceId, ct);
        if (invoice is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.INVOICE_NOT_FOUND);
        }

        // Refused before the credit moves, so a refusal leaves the balance untouched.
        invoice.EnsureCancellable();

        if (invoice.Credit > 0)
        {
            var returned = await _clientCredit.ReturnToBalanceAsync(invoice, invoice.Credit, ct);
            if (returned.IsFailure)
            {
                return returned;
            }
        }

        invoice.Cancel();
        await _activity.RecordAsync(
            new ActivityLogEntry(invoice.ClientId, $"Cancelled Invoice - Invoice ID: {invoice.Id}"), ct);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
