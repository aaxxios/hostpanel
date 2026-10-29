using System.Globalization;
using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Shared.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Support.Queries.ListClientTickets;

/// <summary>Returns a paginated list of tickets for a specific client with department names resolved.</summary>
public sealed class ListClientTicketsHandler
{
    /// <summary>Widest page allowed, regardless of what the caller asks for.</summary>
    private const int MaxPageSize = PagingLimits.MaxPageSize;

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Wolverine bus, used to name the tickets' client through the Clients module.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="bus">Wolverine bus, used to name the tickets' client through the Clients module.</param>
    public ListClientTicketsHandler(IHostpanelDbContext context, IMessageBus bus)
    {
        _context = context;
        _bus = bus;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The client, paging window and optional subject search.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The page of ticket summaries. This use case answers no refusal code.</returns>
    public async Task<Result<PaginatedList<TicketListItemDto>>> HandleAsync(
        ListClientTicketsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, PagingLimits.MinPageSize, MaxPageSize);

        var tickets = _context.Tickets.AsNoTracking().Where(t => t.ClientId == query.ClientId);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            // EF.Functions.Like rather than the Npgsql-only ILike: Application does not reference
            // the Npgsql provider package, and lower-casing both sides gives the same
            // case-insensitive match ListTicketsHandler already relies on.
            var pattern = $"%{query.Search.ToLower(CultureInfo.InvariantCulture)}%";
            tickets = tickets.Where(t => EF.Functions.Like(t.Subject.ToLower(), pattern));
        }

        tickets = tickets.OrderByDescending(t => t.CreatedAt);

        var totalCount = await tickets.CountAsync(ct);
        var items = await tickets
            .Paginate(page, pageSize)
            .GroupJoin(
                _context.Departments.AsNoTracking(),
                t => t.DepartmentId,
                d => (int?)d.Id,
                (t, departments) => new { Ticket = t, Departments = departments })
            .SelectMany(
                x => x.Departments.DefaultIfEmpty(),
                (x, department) => new TicketListItemDto(
                    x.Ticket.Id,
                    x.Ticket.Subject,
                    x.Ticket.Status,
                    x.Ticket.Priority,
                    x.Ticket.CreatedAt,
                    x.Ticket.Replies.Count,
                    department != null ? department.Name : null,
                    x.Ticket.Replies.Count > 0 ? x.Ticket.Replies.Max(r => r.CreatedAt) : (DateTimeOffset?)null,
                    x.Ticket.IsFlagged,
                    x.Ticket.ClientId,
                    string.Empty))
            .ToListAsync(ct);

        // FK_Tickets_Clients_ClientId restricts deleting a client that has tickets, so a page that
        // holds a ticket names a client that exists; an empty page asks for nobody.
        var clientIds = items.Select(t => t.ClientId).Distinct().ToList();
        var clients = await _bus.GetRequiredClientBriefsByIdAsync(clientIds, ct);
        var named = items.Select(t => t with { ClientName = clients[t.ClientId].FullName }).ToList();

        return Result.Success(new PaginatedList<TicketListItemDto>(named, totalCount, page, pageSize));
    }
}
