using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Shared.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Domains.Queries.ListDomains;

/// <summary>Returns a paginated list of all domains for the admin view.</summary>
public sealed class ListDomainsHandler
{
    /// <summary>The data context the page and the client names are read from.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The data context the page and the client names are read from.</param>
    public ListDomainsHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles <see cref="ListDomainsQuery"/>.
    /// </summary>
    /// <param name="query">The list domains query with pagination parameters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A paged result containing domain registration items with client names. Never refuses.</returns>
    public async Task<Result<PagedResult<DomainRegistrationDto>>> HandleAsync(
        ListDomainsQuery query, CancellationToken ct)
    {
        var domainsQuery = _context.Domains.AsQueryable();
        if (query.ClientId.HasValue)
        {
            domainsQuery = domainsQuery.Where(x => x.ClientId == query.ClientId.Value);
        }

        var orderedDomains = domainsQuery.OrderBy(x => x.Name);
        var totalCount = await orderedDomains.CountAsync(ct);
        var items = await orderedDomains
            .Paginate(query.Page, query.PageSize)
            .ToListAsync(ct);

        // Batch-resolve client names
        var clientIds = items.Select(d => d.ClientId).Distinct().ToList();
        var clients = new Dictionary<int, string>();
        foreach (var cid in clientIds)
        {
            var client = await _context.Clients
                .IncludeChildren()
                .FirstOrDefaultAsync(c => c.Id == cid, ct);
            if (client is not null)
            {
                clients[cid] = $"{client.FirstName} {client.LastName}";
            }
        }

        var dtos = items
            .Select(d => new DomainRegistrationDto(
                d.Id,
                d.ClientId,
                clients.GetValueOrDefault(d.ClientId, "Unknown"),
                d.Name,
                FormatRegPeriod(d.RegistrationPeriod),
                d.Registrar,
                d.RecurringAmount,
                d.PriceCurrency,
                d.NextDueDate,
                d.ExpiresAt,
                d.Status,
                d.AutoRenew))
            .ToList();

        var page = new PagedResult<DomainRegistrationDto>(dtos, totalCount, query.Page, query.PageSize);
        return Result.Success(page);
    }

    /// <summary>Formats registration period years as a label.</summary>
    /// <param name="years">Number of years.</param>
    /// <returns>Label like "1 Year" or "2 Years".</returns>
    private static string FormatRegPeriod(int years) =>
        years <= 0 ? "N/A" : years == 1 ? "1 Year" : $"{years} Years";
}
