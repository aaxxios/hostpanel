using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Billing.Extensions;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Shared.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
namespace Innovayse.Hostpanel.Application.Billing.Queries.ListInvoices;

/// <summary>Returns a paginated, optionally filtered list of all invoices for admin consumption.</summary>
public sealed class ListInvoicesHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public ListInvoicesHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The list invoices query.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with a page of invoice list items. Never refuses.</returns>
    public async Task<Result<PagedResult<InvoiceListItemDto>>> HandleAsync(
        ListInvoicesQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        InvoiceStatus? status = null;
        if (!string.IsNullOrEmpty(query.Status) && Enum.TryParse<InvoiceStatus>(query.Status, out var parsed))
        {
            status = parsed;
        }

        IQueryable<Invoice> filtered = _context.Invoices.AsNoTracking();

        if (status.HasValue)
        {
            filtered = filtered.Where(x => x.Status == status.Value);
        }

        if (query.From.HasValue)
        {
            filtered = filtered.Where(x => x.CreatedAt >= query.From.Value);
        }

        if (query.To.HasValue)
        {
            filtered = filtered.Where(x => x.CreatedAt <= query.To.Value);
        }

        var ordered = filtered.OrderByDescending(x => x.CreatedAt);
        var total = await ordered.CountAsync(ct);
        var items = await ordered
            .Paginate(page, pageSize)
            .SelectDto()
            .ToListAsync(ct);

        var clientIds = items.Select(i => i.ClientId).Distinct().ToList();
        var clientMap = await _context.Clients
            .AsNoTracking()
            .Where(c => clientIds.Contains(c.Id))
            .Select(c => new { c.Id, Name = (c.FirstName + " " + c.LastName).Trim() })
            .ToDictionaryAsync(c => c.Id, c => c.Name, ct);

        var dtos = items
            .Select(inv => inv with
            {
                ClientName = clientMap.TryGetValue(inv.ClientId, out var name) ? name : $"Client #{inv.ClientId}",
            })
            .ToList();

        return Result<PagedResult<InvoiceListItemDto>>.Success(new PagedResult<InvoiceListItemDto>(
            dtos, total, page, pageSize));
    }
}
