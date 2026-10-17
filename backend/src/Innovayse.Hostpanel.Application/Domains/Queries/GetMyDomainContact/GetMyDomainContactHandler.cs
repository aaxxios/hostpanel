using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Application.Domains.Queries.GetDomainContact;
using Innovayse.Hostpanel.Shared.Models;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Domains.Queries.GetMyDomainContact;

/// <summary>
/// Reads the registrant contact of one of the calling client's own domains, refusing every id that
/// is not theirs.
/// </summary>
/// <remarks>
/// The ownership check lives here so it travels with the message; the read itself is the same
/// <see cref="GetDomainContactQuery"/> the admin route dispatches.
/// </remarks>
public sealed class GetMyDomainContactHandler
{
    /// <summary>The rule that says a client may only touch their own domains.</summary>
    private readonly IDomainOwnership _ownership;

    /// <summary>Wolverine bus, used to reach the shared read once ownership is settled.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes a new instance of <see cref="GetMyDomainContactHandler"/>.</summary>
    /// <param name="ownership">The rule that says a client may only touch their own domains.</param>
    /// <param name="bus">Wolverine bus, used to reach the shared read once ownership is settled.</param>
    public GetMyDomainContactHandler(IDomainOwnership ownership, IMessageBus bus)
    {
        _ownership = ownership;
        _bus = bus;
    }

    /// <summary>Handles <see cref="GetMyDomainContactQuery"/>.</summary>
    /// <param name="query">The query. It names no account: this reads the caller's own domain.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The registrant contact, or <c>DOMAIN_NOT_FOUND</c> when the domain is not the caller's, or
    /// <c>DOMAIN_CONTACT_NOT_FOUND</c> when the registrar holds no contact record for it.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the request carries no subject.</exception>
    public async Task<Result<DomainContactDto>> HandleAsync(GetMyDomainContactQuery query, CancellationToken ct)
    {
        var ownershipOutcome = await _ownership.RequireOwnedByCallerAsync(query.DomainId, ct);
        if (ownershipOutcome.IsFailure)
        {
            return Result<DomainContactDto>.FailureCode(ownershipOutcome.ErrorCode!);
        }

        return await _bus.InvokeAsync<Result<DomainContactDto>>(new GetDomainContactQuery(query.DomainId), ct);
    }
}
