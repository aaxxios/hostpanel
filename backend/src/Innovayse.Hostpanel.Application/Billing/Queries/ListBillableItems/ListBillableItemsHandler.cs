using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Shared.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
namespace Innovayse.Hostpanel.Application.Billing.Queries.ListBillableItems;

/// <summary>Returns a paginated list of billable items with client names.</summary>
public sealed class ListBillableItemsHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public ListBillableItemsHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The list billable items query.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with a page of billable item DTOs. Never refuses.</returns>
    public async Task<Result<PagedResult<BillableItemDto>>> HandleAsync(
        ListBillableItemsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        IQueryable<BillableItem> filtered = _context.BillableItems.AsNoTracking();
        if (query.Type == nameof(BillableItemType.Recurring))
        {
            filtered = filtered.Where(x => x.Type == BillableItemType.Recurring);
        }
        else if (query.Type == nameof(BillableItemType.OneTime))
        {
            filtered = filtered.Where(x => x.Type == BillableItemType.OneTime);
        }

        var ordered = filtered.OrderByDescending(x => x.CreatedAt);
        var total = await ordered.CountAsync(ct);
        var items = await ordered
            .Paginate(page, pageSize)
            .Select(item => new BillableItemDto(
                item.Id, item.ClientId, string.Empty, item.Description, item.Amount, item.Currency,
                item.Type.ToString(), item.RecurringPeriod, item.IsInvoiced, item.InvoiceId, item.NextDueDate,
                item.CreatedAt))
            .ToListAsync(ct);

        var clientIds = items.Select(i => i.ClientId).Distinct().ToList();
        var clientMap = await _context.Clients
            .AsNoTracking()
            .Where(c => clientIds.Contains(c.Id))
            .Select(c => new { c.Id, Name = (c.FirstName + " " + c.LastName).Trim() })
            .ToDictionaryAsync(c => c.Id, c => c.Name, ct);

        var dtos = items
            .Select(i => i with { ClientName = clientMap.TryGetValue(i.ClientId, out var name) ? name : "Unknown" })
            .ToList();

        return Result<PagedResult<BillableItemDto>>.Success(
            new PagedResult<BillableItemDto>(dtos, total, page, pageSize));
    }
}
