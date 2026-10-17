using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Billing.Queries.GetInvoice;
using Innovayse.Hostpanel.Shared.Models;
using Wolverine;
using Innovayse.Hostpanel.Application.Common.Interfaces;

namespace Innovayse.Hostpanel.Application.Billing.Queries.GetMyInvoice;
/// <summary>
/// Returns one of the calling client's own invoices, refusing every id that is not theirs.
/// </summary>
/// <remarks>
/// The check lives here rather than at the endpoint, so it travels with the message: nothing can
/// read an invoice through <see cref="GetMyInvoiceQuery"/> without it having run. Once ownership
/// is settled the projection is the same read the admin route performs, so this dispatches
/// <see cref="GetInvoiceQuery"/> rather than growing a second copy of the mapping.
/// </remarks>
public sealed class GetMyInvoiceHandler
{
    /// <summary>The rule that says a client may only read their own invoices.</summary>
    private readonly IInvoiceOwnership _ownership;

    /// <summary>Wolverine bus, used to reach the shared read once ownership is settled.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes a new instance of <see cref="GetMyInvoiceHandler"/>.</summary>
    /// <param name="ownership">The rule that says a client may only read their own invoices.</param>
    /// <param name="bus">Wolverine bus, used to reach the shared read once ownership is settled.</param>
    public GetMyInvoiceHandler(IInvoiceOwnership ownership, IMessageBus bus)
    {
        _ownership = ownership;
        _bus = bus;
    }

    /// <summary>Handles <see cref="GetMyInvoiceQuery"/>.</summary>
    /// <param name="query">The query. It names no account: this reads the caller's own invoice.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The matching <see cref="InvoiceDto"/>, or <c>INVOICE_NOT_FOUND</c> (404) when the invoice
    /// is not the caller's, when no such invoice exists, or when the caller has no client record.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the request carries no subject.</exception>
    public async Task<Result<InvoiceDto>> HandleAsync(GetMyInvoiceQuery query, CancellationToken ct)
    {
        var ownershipOutcome = await _ownership.RequireOwnedByCallerAsync(query.InvoiceId, ct);
        if (ownershipOutcome.IsFailure)
        {
            return Result<InvoiceDto>.FailureCode(ownershipOutcome.ErrorCode!);
        }

        return await _bus.InvokeAsync<Result<InvoiceDto>>(new GetInvoiceQuery(query.InvoiceId), ct);
    }
}
