using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Commands.RemoveInvoiceCredit;

/// <summary>Removes a specific credit amount from an invoice.</summary>
public sealed class RemoveInvoiceCreditHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public RemoveInvoiceCreditHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles <see cref="RemoveInvoiceCreditCommand"/>.</summary>
    /// <param name="cmd">The remove credit command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>INVOICE_NOT_FOUND</c> when the id names no invoice.</returns>
    public async Task<Result> HandleAsync(RemoveInvoiceCreditCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var invoice = await _context.Invoices
            .Include(i => i.Items)
            .Include(i => i.Transactions)
            .FirstOrDefaultAsync(i => i.Id == cmd.InvoiceId, ct);
        if (invoice is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.INVOICE_NOT_FOUND);
        }

        invoice.RemoveCredit(cmd.Amount);
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
