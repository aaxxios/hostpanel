using Innovayse.Hostpanel.Application.Billing.Queries.GetInvoicePdf;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Billing.Queries.GetMyInvoicePdf;

/// <summary>
/// Renders one of the calling client's own published invoices as a PDF, refusing every id that is
/// not theirs or is still a draft.
/// </summary>
/// <remarks>
/// <para>
/// Ownership is checked here rather than at the endpoint, so it travels with the message, as in
/// <c>GetMyInvoiceHandler</c>. Once settled, the rendering is the staff use case's, so this
/// dispatches <see cref="GetInvoicePdfQuery"/> rather than growing a second copy of it.
/// </para>
/// <para>
/// <b>Drafts are admin-only.</b> A client is not meant to see an invoice before it is published,
/// so the caller's own draft answers <c>INVOICE_NOT_FOUND</c>, the same as a stranger's invoice
/// or an id that names nothing, and the response does not reveal that a draft exists.
/// </para>
/// </remarks>
public sealed class GetMyInvoicePdfHandler
{
    /// <summary>The rule that says a client may only read their own invoices.</summary>
    private readonly IInvoiceOwnership _ownership;

    /// <summary>The module's data context, for reading the invoice's status.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Wolverine bus, used to reach the shared rendering once access is settled.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the handler.</summary>
    /// <param name="ownership">The rule that says a client may only read their own invoices.</param>
    /// <param name="context">The module's data context, for reading the invoice's status.</param>
    /// <param name="bus">Wolverine bus, used to reach the shared rendering once access is settled.</param>
    public GetMyInvoicePdfHandler(IInvoiceOwnership ownership, IHostpanelDbContext context, IMessageBus bus)
    {
        _ownership = ownership;
        _context = context;
        _bus = bus;
    }

    /// <summary>Handles <see cref="GetMyInvoicePdfQuery"/>.</summary>
    /// <param name="query">The query. It names no account: this reads the caller's own invoice.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The PDF, or <c>INVOICE_NOT_FOUND</c> (404) when the invoice is not the caller's, is a draft,
    /// does not exist, or the caller has no client record.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the request carries no subject.</exception>
    public async Task<Result<FileDownloadDto>> HandleAsync(GetMyInvoicePdfQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var ownershipOutcome = await _ownership.RequireOwnedByCallerAsync(query.InvoiceId, ct);
        if (ownershipOutcome.IsFailure)
        {
            return Result<FileDownloadDto>.FailureCode(ownershipOutcome.ErrorCode!);
        }

        var status = await _context.Invoices
            .AsNoTracking()
            .Where(i => i.Id == query.InvoiceId)
            .Select(i => (InvoiceStatus?)i.Status)
            .FirstOrDefaultAsync(ct);
        if (status is null or InvoiceStatus.Draft)
        {
            return Result<FileDownloadDto>.FailureCode(HostpanelErrorCodes.INVOICE_NOT_FOUND);
        }

        return await _bus.InvokeAsync<Result<FileDownloadDto>>(new GetInvoicePdfQuery(query.InvoiceId), ct);
    }
}
