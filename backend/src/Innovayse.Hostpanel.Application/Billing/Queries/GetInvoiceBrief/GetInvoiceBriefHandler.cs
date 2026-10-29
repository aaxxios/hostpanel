using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Queries.GetInvoiceBrief;

/// <summary>Handles <see cref="GetInvoiceBriefQuery"/>: one read of an invoice's status, total, amount due and currency.</summary>
public sealed class GetInvoiceBriefHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetInvoiceBriefHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The invoice to describe.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The invoice's brief, or <c>INVOICE_NOT_FOUND</c> when the id names no invoice.</returns>
    public async Task<Result<InvoiceBriefDto>> HandleAsync(GetInvoiceBriefQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        // Payments are loaded because what is still due is the invoice's own sum, not a column.
        var invoice = await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Transactions)
            .FirstOrDefaultAsync(i => i.Id == query.InvoiceId, ct);
        if (invoice is null)
        {
            return Result<InvoiceBriefDto>.FailureCode(HostpanelErrorCodes.INVOICE_NOT_FOUND);
        }

        return Result.Success(new InvoiceBriefDto(
            invoice.Id, invoice.ClientId, invoice.Status, invoice.Total, invoice.AmountDue, invoice.Currency, invoice.IsPaidInFull));
    }
}
