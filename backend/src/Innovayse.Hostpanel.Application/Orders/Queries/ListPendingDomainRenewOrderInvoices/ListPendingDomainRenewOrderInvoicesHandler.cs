using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Orders.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Orders.Queries.ListPendingDomainRenewOrderInvoices;

/// <summary>
/// Handles <see cref="ListPendingDomainRenewOrderInvoicesQuery"/>: orders still Pending (not fulfilled,
/// not cancelled, not fraud) with an invoice and a renew line for the name. A fulfilled order is Active
/// and a cancelled one Cancelled, so neither is listed.
/// </summary>
public sealed class ListPendingDomainRenewOrderInvoicesHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public ListPendingDomainRenewOrderInvoicesHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Lists the invoice ids.</summary>
    /// <param name="query">The client and the domain name.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The distinct invoice ids, ordered. Never refuses.</returns>
    public async Task<Result<IReadOnlyList<int>>> HandleAsync(ListPendingDomainRenewOrderInvoicesQuery query, CancellationToken ct)
    {
        var invoiceIds = await _context.Orders.AsNoTracking()
            .Where(o => o.ClientId == query.ClientId
                        && o.Status == OrderStatus.Pending
                        && o.InvoiceId != null
                        && o.Items.Any(l => l.DomainAction == DomainAction.Renew && l.Domain == query.DomainName))
            .Select(o => o.InvoiceId!.Value)
            .Distinct()
            .OrderBy(id => id)
            .ToListAsync(ct);

        return Result<IReadOnlyList<int>>.Success(invoiceIds);
    }
}
