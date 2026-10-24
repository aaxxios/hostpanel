using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Shared.Models;
using Innovayse.Hostpanel.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Queries.GetMyTickets;

/// <summary>
/// Returns every ticket belonging to the calling client, mapped to <see cref="TicketListItemDto"/>.
/// </summary>
public sealed class GetMyTicketsHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Who is asking; the query does not say, and must not.</summary>
    private readonly ICurrentUserService _caller;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="caller">Who is asking.</param>
    public GetMyTicketsHandler(IHostpanelDbContext context, ICurrentUserService caller)
    {
        _context = context;
        _caller = caller;
    }

    /// <summary>Handles <see cref="GetMyTicketsQuery"/>.</summary>
    /// <param name="query">The query. It names no account: this reads the caller's own.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success with the client's ticket summaries, or <c>CLIENT_PROFILE_NOT_FOUND</c> when the
    /// caller has no client record. Answering an empty list instead would make "you have no
    /// account here" indistinguishable from "you have no tickets".
    /// </returns>
    public async Task<Result<IReadOnlyList<TicketListItemDto>>> HandleAsync(GetMyTicketsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var userId = _caller.RequireUserId();

        // Was IClientRepository.FindByUserIdAsync -- read-only, so no tracking is needed.
        var client = await _context.Clients.FindReadOnlyByUserIdAsync(userId, ct);
        if (client is null)
        {
            return Result<IReadOnlyList<TicketListItemDto>>.FailureCode(HostpanelErrorCodes.CLIENT_PROFILE_NOT_FOUND);
        }

        var tickets = await _context.Tickets
            .AsNoTracking()
            .Where(t => t.ClientId == client.Id)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TicketListItemDto(
                t.Id,
                t.Subject,
                t.Status.ToString(),
                t.Priority.ToString(),
                t.CreatedAt,
                t.Replies.Count,
                null,
                t.Replies.Count > 0 ? t.Replies.Max(r => r.CreatedAt) : (DateTimeOffset?)null,
                t.IsFlagged,
                t.ClientId,
                null))
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<TicketListItemDto>>(tickets);
    }
}
