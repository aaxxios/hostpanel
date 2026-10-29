using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Billing.Extensions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Queries.GetMyInvoice;

/// <summary>
/// Returns one of the calling client's own invoices, refusing every id that is not theirs.
/// </summary>
/// <remarks>
/// The check lives here rather than at the endpoint, so it travels with the message: nothing can
/// read an invoice through <see cref="GetMyInvoiceQuery"/> without it having run. The projection is
/// the client's own <see cref="MyInvoiceDto"/>, never the staff read, so no gateway reference or
/// staff note reaches the client.
/// </remarks>
public sealed class GetMyInvoiceHandler
{
    /// <summary>The rule that says a client may only read their own invoices.</summary>
    private readonly IInvoiceOwnership _ownership;

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes a new instance of <see cref="GetMyInvoiceHandler"/>.</summary>
    /// <param name="ownership">The rule that says a client may only read their own invoices.</param>
    /// <param name="context">The module's data context.</param>
    public GetMyInvoiceHandler(IInvoiceOwnership ownership, IHostpanelDbContext context)
    {
        _ownership = ownership;
        _context = context;
    }

    /// <summary>Handles <see cref="GetMyInvoiceQuery"/>.</summary>
    /// <param name="query">The query. It names no account: this reads the caller's own invoice.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The matching <see cref="MyInvoiceDto"/>, or <c>INVOICE_NOT_FOUND</c> (404) when the invoice
    /// is not the caller's, when no such invoice exists, or when the caller has no client record.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the request carries no subject.</exception>
    public async Task<Result<MyInvoiceDto>> HandleAsync(GetMyInvoiceQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var ownershipOutcome = await _ownership.RequireOwnedByCallerAsync(query.InvoiceId, ct);
        if (ownershipOutcome.IsFailure)
        {
            return Result<MyInvoiceDto>.FailureCode(ownershipOutcome.ErrorCode!);
        }

        var invoice = await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Items)
            .Include(i => i.Transactions)
            .FirstOrDefaultAsync(i => i.Id == query.InvoiceId, ct);
        if (invoice is null)
        {
            return Result<MyInvoiceDto>.FailureCode(HostpanelErrorCodes.INVOICE_NOT_FOUND);
        }

        return Result<MyInvoiceDto>.Success(InvoiceMapper.MapToMyDto(invoice));
    }
}
