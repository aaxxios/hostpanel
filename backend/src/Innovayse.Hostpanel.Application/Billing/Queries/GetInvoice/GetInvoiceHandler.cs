using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Billing.Extensions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Queries.GetInvoice;

/// <summary>Returns a full <see cref="InvoiceDto"/> including line items, transactions, and client name.</summary>
public sealed class GetInvoiceHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetInvoiceHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The get invoice query.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success with the invoice DTO, or <c>INVOICE_NOT_FOUND</c> when the id names no invoice.
    /// </returns>
    public async Task<Result<InvoiceDto>> HandleAsync(GetInvoiceQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var invoice = await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Items)
            .Include(i => i.Transactions)
            .FirstOrDefaultAsync(i => i.Id == query.InvoiceId, ct);

        if (invoice is null)
        {
            return Result<InvoiceDto>.FailureCode(HostpanelErrorCodes.INVOICE_NOT_FOUND);
        }

        var clientName = await _context.Clients
            .AsNoTracking()
            .Where(c => c.Id == invoice.ClientId)
            .Select(c => c.FirstName + " " + c.LastName)
            .FirstOrDefaultAsync(ct) ?? $"Client #{invoice.ClientId}";

        return Result<InvoiceDto>.Success(InvoiceMapper.MapToDto(invoice, clientName));
    }
}
