using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Commands.UpdateInvoiceItems;

/// <summary>Applies item additions, updates, and deletions to an invoice.</summary>
public sealed class UpdateInvoiceItemsHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public UpdateInvoiceItemsHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The update invoice items command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, or <c>INVOICE_NOT_FOUND</c> when the id names no invoice. An item id that names no
    /// line on the invoice, or an invoice no longer editable, still throws — see
    /// <see cref="Domain.Billing.Invoice.UpdateItem"/> and <see cref="Domain.Billing.Invoice.AddItem"/>.
    /// </returns>
    public async Task<Result> HandleAsync(UpdateInvoiceItemsCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var invoice = await _context.Invoices
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == cmd.InvoiceId, ct);
        if (invoice is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.INVOICE_NOT_FOUND);
        }

        foreach (var entry in cmd.Items)
        {
            if (entry.Id is null && !entry.IsDeleted)
            {
                invoice.AddItem(entry.Description, entry.UnitPrice, entry.Quantity);
            }
            else if (entry.Id.HasValue && entry.IsDeleted)
            {
                invoice.RemoveItem(entry.Id.Value);
            }
            else if (entry.Id.HasValue && !entry.IsDeleted)
            {
                invoice.UpdateItem(entry.Id.Value, entry.Description, entry.UnitPrice, entry.Quantity);
            }
        }

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
