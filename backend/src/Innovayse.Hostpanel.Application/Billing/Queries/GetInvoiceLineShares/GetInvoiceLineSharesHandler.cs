using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Queries.GetInvoiceLineShares;

/// <summary>Handles <see cref="GetInvoiceLineSharesQuery"/>: one read of how an invoice line was paid.</summary>
public sealed class GetInvoiceLineSharesHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetInvoiceLineSharesHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The invoice and the line amount to split.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The line's shares, or <c>INVOICE_NOT_FOUND</c> when the id names no invoice.</returns>
    public async Task<Result<InvoiceLineSharesDto>> HandleAsync(GetInvoiceLineSharesQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        // Payments and refunds are loaded because the refunded sum is the invoice's own, not a column.
        var invoice = await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Transactions)
            .FirstOrDefaultAsync(i => i.Id == query.InvoiceId, ct);
        if (invoice is null)
        {
            return Result<InvoiceLineSharesDto>.FailureCode(HostpanelErrorCodes.INVOICE_NOT_FOUND);
        }

        return Result.Success(new InvoiceLineSharesDto(
            invoice.ShareOfCredit(query.LineAmount), invoice.ShareOfTotal(query.LineAmount), invoice.AmountRefunded));
    }
}
