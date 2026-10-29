using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Billing.Extensions;
using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Billing.Queries.ListClientInvoices;

/// <summary>Returns a paginated, filtered list of invoices for a specific client.</summary>
public sealed class ListClientInvoicesHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>The message bus, for the Clients module's client briefs; Billing never reads the client tables.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="bus">The message bus, for the Clients module's client briefs; Billing never reads the client tables.</param>
    public ListClientInvoicesHandler(IHostpanelDbContext context, IMessageBus bus)
    {
        _context = context;
        _bus = bus;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The list client invoices query.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with a page of invoice list items for the client. Never refuses.</returns>
    public async Task<Result<PaginatedList<InvoiceListItemDto>>> HandleAsync(
        ListClientInvoicesQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, PagingLimits.MinPageSize, PagingLimits.MaxPageSize);

        var filtered = _context.Invoices.AsNoTracking().Where(x => x.ClientId == query.ClientId);

        if (query.Status.HasValue)
        {
            filtered = filtered.Where(x => x.Status == query.Status.Value);
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

        var clientIds = items.Select(inv => inv.ClientId).Distinct().ToList();
        var clientMap = await _bus.GetRequiredClientBriefsByIdAsync(clientIds, ct);

        var dtos = items.Select(inv => inv with { ClientName = clientMap[inv.ClientId].FullName }).ToList();

        return Result<PaginatedList<InvoiceListItemDto>>.Success(new PaginatedList<InvoiceListItemDto>(
            dtos, total, page, pageSize));
    }
}
