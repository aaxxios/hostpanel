using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Common.Interfaces;

/// <summary>
/// Enforces that a domain belongs to the calling client.
/// </summary>
/// <remarks>
/// <para>
/// The client-facing domain routes may only ever touch the caller's own domains, and the rule
/// that decides that used to run in <c>MyDomainsController</c> -- eighteen times, each one an
/// <c>if (!await ownership.IsOwnedByCallerAsync(id, ct)) return Forbid();</c> ahead of the
/// dispatch. That was better than reading claims in the controller, but the check still
/// travelled separately from the message: every one of the commands behind those actions is
/// also dispatched by the admin <c>DomainsController</c>, so the guarantee held only for
/// callers who happened to come through that one endpoint.
/// </para>
/// <para>
/// It now sits beside <c>ITicketOwnership</c> and <c>IInvoiceOwnership</c> and is shaped like
/// them: it resolves the caller through <c>ICurrentUserService</c> rather than being told
/// whom to check. Unlike those two, which still throw a typed exception, this one answers a
/// <see cref="Result"/> carrying <c>DOMAIN_NOT_FOUND</c> on refusal, so every client-facing
/// domain handler can propagate it the same way it propagates any other refusal — no thrown
/// exception the API boundary has to know about specially. The client-facing <c>My*</c> handlers
/// call it; the endpoint no longer does.
/// </para>
/// </remarks>
public interface IDomainOwnership
{
    /// <summary>
    /// Verifies that the domain belongs to the client the current credential names.
    /// </summary>
    /// <param name="domainId">Domain primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success when the domain is the caller's own, or a failed <see cref="Result"/> carrying
    /// <c>DOMAIN_NOT_FOUND</c> when the domain belongs to somebody else, when no such domain
    /// exists, or when the caller has no client record. The three answer alike on purpose:
    /// telling them apart would let the route be used to enumerate domain ids.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the request carries no subject.</exception>
    Task<Result> RequireOwnedByCallerAsync(int domainId, CancellationToken ct);
}
