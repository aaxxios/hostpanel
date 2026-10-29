using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Common;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Domains.Queries.GetDomainRenewalStates;

/// <summary>Handles <see cref="GetDomainRenewalStatesQuery"/> with one read.</summary>
public sealed class GetDomainRenewalStatesHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetDomainRenewalStatesHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Reads the states.</summary>
    /// <param name="query">The domains.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>One state per existing domain, ordered by id. Never refuses.</returns>
    public async Task<Result<IReadOnlyList<DomainRenewalStateDto>>> HandleAsync(GetDomainRenewalStatesQuery query, CancellationToken ct)
    {
        if (query.DomainIds.Count == 0)
        {
            return Result<IReadOnlyList<DomainRenewalStateDto>>.Success([]);
        }

        var states = await _context.Domains.AsNoTracking()
            .Where(d => query.DomainIds.Contains(d.Id))
            .OrderBy(d => d.Id)
            .Select(d => new DomainRenewalStateDto(d.Id, d.Name, d.ExpiresAt, d.Status, d.RegistrarRef != null))
            .ToListAsync(ct);

        return Result<IReadOnlyList<DomainRenewalStateDto>>.Success(states);
    }
}
