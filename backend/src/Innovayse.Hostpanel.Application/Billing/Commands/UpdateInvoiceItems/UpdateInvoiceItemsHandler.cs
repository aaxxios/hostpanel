using Innovayse.Hostpanel.Application.Billing.Extensions;
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

    /// <summary>Returns credit the change left unneeded to the client's balance in the same save.</summary>
    private readonly IClientCredit _clientCredit;

    /// <summary>Stamps the moment a change that leaves the payments covering the invoice settles it.</summary>
    private readonly IClock _clock;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="clientCredit">Returns credit the change left unneeded to the client's balance in the same save.</param>
    /// <param name="clock">Stamps the moment a change that leaves the payments covering the invoice settles it.</param>
    public UpdateInvoiceItemsHandler(IHostpanelDbContext context, IClientCredit clientCredit, IClock clock)
    {
        _context = context;
        _clientCredit = clientCredit;
        _clock = clock;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The update invoice items command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, <c>INVOICE_NOT_FOUND</c> when the id names no invoice, or <c>CLIENT_NOT_FOUND</c> when
    /// excess credit has no client to go back to. An item id that names no line on the invoice, an
    /// invoice no longer editable, or a change below the payments taken still throws; see
    /// <see cref="Domain.Billing.Invoice.UpdateItem"/> and <see cref="Domain.Billing.Invoice.AddItem"/>.
    /// </returns>
    public async Task<Result> HandleAsync(UpdateInvoiceItemsCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var invoice = await _context.Invoices
            .Include(x => x.Items)
            .Include(x => x.Transactions)
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

        // A lower charge can leave applied credit the invoice no longer needs; it goes back to the
        // balance in this save so credit is never parked under a total it exceeds.
        if (invoice.ExcessCredit > 0)
        {
            var returned = await _clientCredit.ReturnToBalanceAsync(invoice, invoice.ExcessCredit, ct);
            if (returned.IsFailure)
            {
                return returned;
            }
        }

        // A lower charge can leave the payments already taken covering the invoice: nothing is owed,
        // so it is settled in this save rather than left open to go overdue.
        invoice.SettleIfCovered(_clock.UtcNow);

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
