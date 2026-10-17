using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Commands.UpdateInvoiceNotes;

/// <summary>Updates or clears the notes on an invoice.</summary>
public sealed class UpdateInvoiceNotesHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public UpdateInvoiceNotesHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The update notes command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>INVOICE_NOT_FOUND</c> when the id names no invoice.</returns>
    public async Task<Result> HandleAsync(UpdateInvoiceNotesCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var invoice = await _context.Invoices.FirstOrDefaultAsync(x => x.Id == cmd.InvoiceId, ct);
        if (invoice is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.INVOICE_NOT_FOUND);
        }

        invoice.UpdateNotes(cmd.Notes);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
