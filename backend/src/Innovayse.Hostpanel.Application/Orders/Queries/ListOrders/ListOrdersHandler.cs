using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Orders;
using Innovayse.Hostpanel.Domain.Orders.Enums;
using Innovayse.Hostpanel.Shared.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Orders.Queries.ListOrders;

/// <summary>
/// Returns a paged list of all orders mapped to <see cref="OrderListItemDto"/>.
/// Batch-fetches client names to avoid N+1 queries.
/// </summary>
public sealed class ListOrdersHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public ListOrdersHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The page, page size and status filter to list by.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with a page of order summaries. Never refuses.</returns>
    public async Task<Result<PagedResult<OrderListItemDto>>> HandleAsync(ListOrdersQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, PagingLimits.MinPageSize, PagingLimits.MaxPageSize);
        var statusFilter = ParseStatus(query.Status);

        IQueryable<Order> filtered = _context.Orders.AsNoTracking();
        if (statusFilter.HasValue)
        {
            filtered = filtered.Where(o => o.Status == statusFilter.Value);
        }

        var ordered = filtered.OrderByDescending(o => o.CreatedAt);
        var totalCount = await ordered.CountAsync(ct);
        var orders = await ordered
            .Paginate(page, pageSize)
            .Include(o => o.Items)
            .ToListAsync(ct);

        var clientIds = orders.Select(o => o.ClientId).Distinct().ToList();
        var clientMap = await _context.Clients
            .AsNoTracking()
            .Where(c => clientIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => $"{c.FirstName} {c.LastName}", ct);

        var items = orders
            .Select(o => new OrderListItemDto(
                o.Id,
                o.OrderNumber,
                o.ClientId,
                clientMap.GetValueOrDefault(o.ClientId),
                o.Status.ToString(),
                o.PaymentMethod,
                o.Items.Sum(i => i.FirstPaymentAmount),
                o.InvoiceId,
                o.Items.Count,
                o.CreatedAt))
            .ToList();

        return Result.Success(new PagedResult<OrderListItemDto>(items, totalCount, page, pageSize));
    }

    /// <summary>
    /// Parses a status filter string to <see cref="OrderStatus"/>.
    /// Returns <see langword="null"/> when the input is null, empty, or not a valid enum value.
    /// </summary>
    /// <param name="status">The status string to parse.</param>
    /// <returns>The parsed <see cref="OrderStatus"/>, or null if parsing fails.</returns>
    private static OrderStatus? ParseStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        return Enum.TryParse<OrderStatus>(status, ignoreCase: true, out var parsed) ? parsed : null;
    }
}
