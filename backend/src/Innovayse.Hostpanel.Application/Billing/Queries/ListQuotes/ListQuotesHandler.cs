using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Billing.Extensions;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Shared.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Queries.ListQuotes;

/// <summary>Returns a paginated list of all quotes for admin consumption.</summary>
public sealed class ListQuotesHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public ListQuotesHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The list quotes query.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with a page of quote list items. Never refuses.</returns>
    public async Task<Result<PagedResult<QuoteListItemDto>>> HandleAsync(
        ListQuotesQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var ordered = _context.Quotes.AsNoTracking().OrderByDescending(x => x.CreatedAt);
        var total = await ordered.CountAsync(ct);
        var items = await ordered
            .Paginate(page, pageSize)
            .SelectDto()
            .ToListAsync(ct);

        var clientIds = items.Select(q => q.ClientId).Distinct().ToList();
        var clientMap = await _context.Clients
            .AsNoTracking()
            .Where(c => clientIds.Contains(c.Id))
            .Select(c => new { c.Id, Name = (c.FirstName + " " + c.LastName).Trim() })
            .ToDictionaryAsync(c => c.Id, c => c.Name, ct);

        var dtos = items
            .Select(q => q with { ClientName = clientMap.TryGetValue(q.ClientId, out var name) ? name : "Unknown" })
            .ToList();

        return Result<PagedResult<QuoteListItemDto>>.Success(
            new PagedResult<QuoteListItemDto>(dtos, total, page, pageSize));
    }
}
