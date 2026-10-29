using Innovayse.Hostpanel.Application.Audit.Common;
using Innovayse.Hostpanel.Application.Billing.Extensions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Commands.UpdateTransaction;

/// <summary>Updates an existing <see cref="Domain.Billing.Transaction"/>.</summary>
public sealed class UpdateTransactionHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Records the activity log line for this action, saved with the change it describes.</summary>
    private readonly IActivityLogger _activity;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="activity">Records the activity log line for this action, saved with the change it describes.</param>
    public UpdateTransactionHandler(IHostpanelDbContext context, IActivityLogger activity)
    {
        _context = context;
        _activity = activity;
    }

    /// <summary>Handles <see cref="UpdateTransactionCommand"/>.</summary>
    /// <param name="cmd">The update command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, <c>TRANSACTION_NOT_FOUND</c> when the id names no transaction, or <c>INVOICE_NOT_FOUND</c>
    /// when it is linked to an invoice that does not exist.
    /// </returns>
    public async Task<Result> HandleAsync(UpdateTransactionCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var transaction = await _context.Transactions.FirstOrDefaultAsync(t => t.Id == cmd.Id, ct);
        if (transaction is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.TRANSACTION_NOT_FOUND);
        }

        if (cmd.InvoiceId is { } invoiceId && !await _context.InvoiceExistsAsync(invoiceId, ct))
        {
            return Result.FailureCode(HostpanelErrorCodes.INVOICE_NOT_FOUND);
        }

        transaction.CorrectDetails(cmd.Date, cmd.Description, cmd.TransactionId, cmd.PaymentMethod);
        transaction.LinkToInvoice(cmd.InvoiceId);
        transaction.CorrectAmounts(cmd.AmountIn, cmd.AmountOut, cmd.Fees);

        await _context.SaveChangesAsync(ct);

        await _activity.RecordAsync(
            new ActivityLogEntry(transaction.ClientId, $"Updated Transaction - Transaction ID: {transaction.Id}"), ct);
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
