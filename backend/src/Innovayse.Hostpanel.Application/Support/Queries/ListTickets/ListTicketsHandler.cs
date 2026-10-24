using System.Globalization;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Domain.Support;
using Innovayse.Hostpanel.Domain.Support.Enums;
using Innovayse.Hostpanel.Shared.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Queries.ListTickets;

/// <summary>
/// Returns a paged list of support tickets, optionally filtered by status or search term, mapped to
/// <see cref="TicketListItemDto"/>.
/// </summary>
public sealed class ListTicketsHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public ListTicketsHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The filters and paging window.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with the requested page. This use case refuses nothing.</returns>
    public async Task<Result<PagedResult<TicketListItemDto>>> HandleAsync(
        ListTicketsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, PagingLimits.MinPageSize, PagingLimits.MaxPageSize);

        var flaggedOnly = string.Equals(query.Status, "flagged", StringComparison.OrdinalIgnoreCase);
        TicketStatus? statusFilter = null;

        if (!flaggedOnly && !string.IsNullOrWhiteSpace(query.Status) &&
            Enum.TryParse<TicketStatus>(query.Status, ignoreCase: true, out var parsed))
        {
            statusFilter = parsed;
        }

        var ticketsQuery = _context.Tickets.AsNoTracking().Include(t => t.Replies).AsQueryable();

        if (statusFilter.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(t => t.Status == statusFilter.Value);
        }

        if (flaggedOnly)
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
        var tickets = await ticketsQuery
            .Paginate(page, pageSize)
            .ToListAsync(ct);

        var departments = await _context.Departments.AsNoTracking().ToListAsync(ct);
        var deptMap = departments.ToDictionary(d => d.Id, d => d.Name);

        var clientIds = tickets.Select(t => t.ClientId).Distinct().ToList();
        var clientNames = await _context.Clients
            .AsNoTracking()
            .Where(c => clientIds.Contains(c.Id))
            .Select(c => new { c.Id, Name = (c.FirstName + " " + c.LastName).Trim() })
            .ToDictionaryAsync(c => c.Id, c => c.Name, ct);

        var items = tickets
            .Select(t => new TicketListItemDto(
                t.Id,
                t.Subject,
                t.Status.ToString(),
                t.Priority.ToString(),
                t.CreatedAt,
                t.Replies.Count,
                t.DepartmentId.HasValue && deptMap.TryGetValue(t.DepartmentId.Value, out var name) ? name : null,
                t.Replies.Count > 0 ? t.Replies.Max(r => r.CreatedAt) : null,
                t.IsFlagged,
                t.ClientId,
                clientNames.GetValueOrDefault(t.ClientId)))
            .ToList();

        return Result.Success(new PagedResult<TicketListItemDto>(items, totalCount, page, pageSize));
    }
}
