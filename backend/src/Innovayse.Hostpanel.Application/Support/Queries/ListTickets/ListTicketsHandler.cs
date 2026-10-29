using System.Globalization;
using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Shared.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Support.Queries.ListTickets;

/// <summary>
/// Returns a paged list of support tickets, optionally filtered by status, flag or search term, mapped to
/// <see cref="TicketListItemDto"/>.
/// </summary>
public sealed class ListTicketsHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Wolverine bus, used to name each ticket's client through the Clients module.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="bus">Wolverine bus, used to name each ticket's client through the Clients module.</param>
    public ListTicketsHandler(IHostpanelDbContext context, IMessageBus bus)
    {
        _context = context;
        _bus = bus;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The filters and paging window.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with the requested page. This use case refuses nothing.</returns>
    public async Task<Result<PaginatedList<TicketListItemDto>>> HandleAsync(
        ListTicketsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, PagingLimits.MinPageSize, PagingLimits.MaxPageSize);

        var ticketsQuery = _context.Tickets.AsNoTracking();

        if (query.Status is { } status)
        {
            ticketsQuery = ticketsQuery.Where(t => t.Status == status);
        }

        if (query.Flagged)
        {
            ticketsQuery = ticketsQuery.Where(t => t.IsFlagged);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{query.Search.ToLower(CultureInfo.InvariantCulture)}%";
            ticketsQuery = ticketsQuery.Where(t => EF.Functions.Like(t.Subject.ToLower(), pattern));
        }

        ticketsQuery = ticketsQuery.OrderByDescending(t => t.CreatedAt);

        var totalCount = await ticketsQuery.CountAsync(ct);

        // The list needs each ticket's reply count and last reply moment, not the replies: both are
        // computed in SQL, so a page never loads the conversations of its tickets.
        var tickets = await ticketsQuery
            .Paginate(page, pageSize)
            .Select(t => new
            {
                t.Id,
                t.Subject,
                t.Status,
                t.Priority,
                t.CreatedAt,
                ReplyCount = t.Replies.Count(),
                LastReplyAt = t.Replies.Max(r => (DateTimeOffset?)r.CreatedAt),
                t.DepartmentId,
                t.IsFlagged,
                t.ClientId,
            })
            .ToListAsync(ct);

        var departments = await _context.Departments.AsNoTracking().ToListAsync(ct);
        var deptMap = departments.ToDictionary(d => d.Id, d => d.Name);

        // FK_Tickets_Clients_ClientId restricts deleting a client that has tickets, so every owner is there.
        var clientIds = tickets.Select(t => t.ClientId).Distinct().ToList();
        var clients = await _bus.GetRequiredClientBriefsByIdAsync(clientIds, ct);

        var items = tickets
            .Select(t => new TicketListItemDto(
                t.Id,
                t.Subject,
                t.Status,
                t.Priority,
                t.CreatedAt,
                t.ReplyCount,
                t.DepartmentId.HasValue && deptMap.TryGetValue(t.DepartmentId.Value, out var name) ? name : null,
                t.LastReplyAt,
                t.IsFlagged,
                t.ClientId,
                clients[t.ClientId].FullName))
            .ToList();

        return Result.Success(new PaginatedList<TicketListItemDto>(items, totalCount, page, pageSize));
    }
}
