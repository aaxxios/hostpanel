using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Queries.ListDomainsOnOpenRenewalInvoices;

/// <summary>
/// Handles <see cref="ListDomainsOnOpenRenewalInvoicesQuery"/>: a domain counts when a renewal line
/// for it sits on an invoice in one of the <see cref="RenewalInvoiceStatuses.Open"/> statuses, the
/// same set the renewal run uses.
/// </summary>
public sealed class ListDomainsOnOpenRenewalInvoicesHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public ListDomainsOnOpenRenewalInvoicesHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The domains to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The ids among the given domains billed by an open renewal invoice, each once. Never refuses.</returns>
    public async Task<Result<IReadOnlyList<int>>> HandleAsync(ListDomainsOnOpenRenewalInvoicesQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.RegisteredDomainIds.Count == 0)
        {
            return Result<IReadOnlyList<int>>.Success([]);
        }

        var ids = query.RegisteredDomainIds;
        var billed = await _context.Invoices
            .AsNoTracking()
            .Where(i => RenewalInvoiceStatuses.Open.Contains(i.Status))
            .SelectMany(i => i.Items)
            .Where(l => l.RegisteredDomainId != null && ids.Contains(l.RegisteredDomainId.Value) && l.RenewalPeriodStart != null)
            .Select(l => l.RegisteredDomainId!.Value)
            .Distinct()
            .ToListAsync(ct);

        return Result<IReadOnlyList<int>>.Success(billed);
    }
}
