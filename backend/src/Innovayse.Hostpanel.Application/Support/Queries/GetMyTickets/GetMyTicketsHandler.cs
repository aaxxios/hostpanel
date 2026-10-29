using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Queries.GetMyTickets;

/// <summary>
/// Returns every ticket of the account the caller acts for, mapped to <see cref="MyTicketListItemDto"/>.
/// </summary>
public sealed class GetMyTicketsHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Resolves the account the caller acts for, owner or linked user; the query does not say, and must not.</summary>
    private readonly IClientAccess _access;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="access">Resolves the account the caller acts for, owner or linked user.</param>
    public GetMyTicketsHandler(IHostpanelDbContext context, IClientAccess access)
    {
        _context = context;
        _access = access;
    }

    /// <summary>Handles <see cref="GetMyTicketsQuery"/>.</summary>
    /// <param name="query">The query. It names no account: this reads the caller's own.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success with the client's ticket summaries, or <c>CLIENT_PROFILE_NOT_FOUND</c> when the
    /// caller acts for no account. Answering an empty list instead would make "you have no
    /// account here" indistinguishable from "you have no tickets".
    /// </returns>
    public async Task<Result<IReadOnlyList<MyTicketListItemDto>>> HandleAsync(GetMyTicketsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var clientId = await _access.RequireClientIdAsync(ct);
        if (clientId.IsFailure)
        {
            return Result<IReadOnlyList<MyTicketListItemDto>>.FailureCode(clientId.ErrorCode!);
        }

        var tickets = await _context.Tickets
            .AsNoTracking()
            .Where(t => t.ClientId == clientId.Value)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new MyTicketListItemDto(
                t.Id,
                t.Subject,
                t.Status,
                t.Priority,
                t.CreatedAt,
                t.Replies.Count,
                null,
                t.Replies.Count > 0 ? t.Replies.Max(r => r.CreatedAt) : (DateTimeOffset?)null))
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<MyTicketListItemDto>>(tickets);
    }
}
