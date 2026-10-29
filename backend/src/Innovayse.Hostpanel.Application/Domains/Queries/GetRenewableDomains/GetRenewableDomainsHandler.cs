using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Domain.Domains.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Domains.Queries.GetRenewableDomains;

/// <summary>Handles <see cref="GetRenewableDomainsQuery"/> with one read.</summary>
public sealed class GetRenewableDomainsHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetRenewableDomainsHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Lists the due domains.</summary>
    /// <param name="query">The client and the window.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The due domains ordered by id. Never refuses.</returns>
    public async Task<Result<IReadOnlyList<RenewableDomainDto>>> HandleAsync(GetRenewableDomainsQuery query, CancellationToken ct)
    {
        var domains = _context.Domains.AsNoTracking()
            .Where(d => d.Status == DomainStatus.Active && d.AutoRenew && d.ExpiresAt <= query.DueBy);
        if (query.ClientId is { } clientId)
        {
            domains = domains.Where(d => d.ClientId == clientId);
        }

        var due = await domains
            .OrderBy(d => d.Id)
            .Select(d => new RenewableDomainDto(d.Id, d.ClientId, d.Name, d.ExpiresAt, d.RegistrationPeriod))
            .ToListAsync(ct);

        return Result<IReadOnlyList<RenewableDomainDto>>.Success(due);
    }
}
