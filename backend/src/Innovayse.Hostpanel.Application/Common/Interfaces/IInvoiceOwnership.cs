using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Common.Interfaces;

/// <summary>
/// Enforces that an invoice belongs to the calling client.
/// </summary>
/// <remarks>
/// <para>
/// The client-facing billing routes used to carry the rule as
/// <c>if (invoice.ClientId != profile.Id) return Forbid();</c> written out four times in
/// <c>MyBillingController</c>. It worked, but it lived at the endpoint: every new action had to
/// remember it, and it travelled separately from the command, so anything that dispatched
/// <c>PayInvoiceCommand</c> or <c>StartGatewayPaymentCommand</c> without going through that
/// controller was unchecked.
/// </para>
/// <para>
/// It now sits in the Application layer beside <c>ITicketOwnership</c> and
/// <c>IDomainOwnership</c>, resolving the caller itself through <c>ICurrentUserService</c>
/// rather than being told whom to check, and is called by the client-facing handlers rather than
/// by the endpoint.
/// </para>
/// <para>
/// The refusal used to be a thrown <c>InvoiceNotFoundException</c>, caught by
/// <c>ExceptionMiddleware</c>. It is now a <see cref="Result"/> the caller must inspect, matching
/// <c>IServiceOwnership</c>: a refusal nobody reads must not become silent, so every caller checks
/// <see cref="Result.IsFailure"/> and forwards <see cref="Result.ErrorCode"/> rather than letting
/// the call simply return.
/// </para>
/// </remarks>
public interface IInvoiceOwnership
{
    /// <summary>
    /// Verifies that the invoice belongs to the client the current credential names.
    /// </summary>
    /// <param name="invoiceId">Invoice primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success when the caller owns the invoice; otherwise a failure carrying
    /// <c>INVOICE_NOT_FOUND</c> (404). The invoice belonging to somebody else, no such invoice
    /// existing, and the caller having no client record all answer alike on purpose: telling them
    /// apart would let the route be used to enumerate invoice ids.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the request carries no subject.</exception>
    Task<Result> RequireOwnedByCallerAsync(int invoiceId, CancellationToken ct);
}
