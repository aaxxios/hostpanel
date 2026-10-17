using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Commands.UpdateInvoiceOptions;

/// <summary>Updates invoice options (dates, payment method, tax rate).</summary>
public sealed class UpdateInvoiceOptionsHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public UpdateInvoiceOptionsHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The update options command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>INVOICE_NOT_FOUND</c> when the id names no invoice.</returns>
    public async Task<Result> HandleAsync(UpdateInvoiceOptionsCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        // Items must be loaded: UpdateOptions recomputes the totals from the current line items,
        // and an untracked, empty collection would zero the sub-total on every save.
        var invoice = await _context.Invoices
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == cmd.InvoiceId, ct);
        if (invoice is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.INVOICE_NOT_FOUND);
        }

        invoice.UpdateOptions(cmd.InvoiceDate, cmd.DueDate, cmd.PaymentMethod, cmd.TaxRate);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
