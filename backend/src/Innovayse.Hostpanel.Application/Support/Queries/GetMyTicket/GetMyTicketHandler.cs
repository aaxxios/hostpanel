using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Application.Support.Queries.GetTicket;
using Innovayse.Hostpanel.Shared.Models;
using Wolverine;
using Innovayse.Hostpanel.Application.Common.Interfaces;

namespace Innovayse.Hostpanel.Application.Support.Queries.GetMyTicket;

/// <summary>
/// Returns one of the calling client's own tickets, refusing every id that is not theirs.
/// </summary>
/// <remarks>
/// The check lives here rather than at the endpoint, so it travels with the message: nothing can
/// read a ticket through <see cref="GetMyTicketQuery"/> without it having run. Once ownership is
/// settled the projection is the same read the admin route performs, so this dispatches
/// <see cref="GetTicketQuery"/> rather than growing a second copy of the mapping that could drift
/// from it.
/// </remarks>
public sealed class GetMyTicketHandler
{
    /// <summary>The rule that says a client may only read their own tickets.</summary>
    private readonly ITicketOwnership _ownership;

    /// <summary>Wolverine bus, used to reach the shared read once ownership is settled.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the handler.</summary>
    /// <param name="ownership">The rule that says a client may only read their own tickets.</param>
    /// <param name="bus">Wolverine bus, used to reach the shared read once ownership is settled.</param>
    public GetMyTicketHandler(ITicketOwnership ownership, IMessageBus bus)
    {
        _ownership = ownership;
        _bus = bus;
    }

    /// <summary>Handles <see cref="GetMyTicketQuery"/>.</summary>
    /// <param name="query">The query. It names no account: this reads the caller's own ticket.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The matching <see cref="TicketDto"/>, or <c>TICKET_NOT_FOUND</c> when the ticket is not the
    /// caller's, when no such ticket exists, or when the caller has no client record.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the request carries no subject.</exception>
    public async Task<Result<TicketDto>> HandleAsync(GetMyTicketQuery query, CancellationToken ct)
    {
        var ownershipOutcome = await _ownership.RequireOwnedByCallerAsync(query.Id, ct);
        if (ownershipOutcome.IsFailure)
        {
            return Result<TicketDto>.FailureCode(ownershipOutcome.ErrorCode!);
        }

        return await _bus.InvokeAsync<Result<TicketDto>>(new GetTicketQuery(query.Id), ct);
    }
}
