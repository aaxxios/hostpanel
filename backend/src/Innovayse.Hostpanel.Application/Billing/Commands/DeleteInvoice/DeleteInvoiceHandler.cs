using Innovayse.Hostpanel.Application.Audit.Common;
using Innovayse.Hostpanel.Application.Billing.Extensions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Commands.DeleteInvoice;

/// <summary>
/// Permanently deletes a Draft or Cancelled invoice. Billable items that were put on it are released
/// to be invoiced again, and ledger rows and orders that point at it keep their own record, losing
/// only the link.
/// </summary>
public sealed class DeleteInvoiceHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Records the activity log line for this action, saved with the change it describes.</summary>
    private readonly IActivityLogger _activity;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="activity">Records the activity log line for this action, saved with the change it describes.</param>
    public DeleteInvoiceHandler(IHostpanelDbContext context, IActivityLogger activity)
    {
        _context = context;
        _activity = activity;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The delete command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>INVOICE_NOT_FOUND</c> when the id names no invoice, or
    /// <c>INVOICE_NOT_DELETABLE</c> when it is not a Draft or Cancelled invoice: one that was
    /// issued for payment stays as a financial record.
    /// </returns>
    public async Task<Result> HandleAsync(DeleteInvoiceCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == cmd.InvoiceId, ct);
        if (invoice is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.INVOICE_NOT_FOUND);
        }

        if (!invoice.CanDelete)
        {
            return Result.FailureCode(HostpanelErrorCodes.INVOICE_NOT_DELETABLE);
        }

        var clientId = invoice.ClientId;
        var invoiceId = invoice.Id;

        // Billable items put on this invoice go back to waiting for invoicing: the next run bills them.
        await _context.ReleaseBillableItemsAsync([invoiceId], ct);
        _context.Invoices.Remove(invoice);
        await _activity.RecordAsync(new ActivityLogEntry(clientId, $"Deleted Invoice - Invoice ID: {invoiceId}"), ct);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
