using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Reports.Common;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Reports.Queries.GetDiskUsageByDomains;

/// <summary>
/// Handles <see cref="GetDiskUsageByDomainsQuery"/>: reads the usage samples of the named domains
/// and keeps the newest one per server and domain.
/// </summary>
public sealed class GetDiskUsageByDomainsHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetDiskUsageByDomainsHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The domains to look up.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The newest sample per server and domain, in no particular order. Never refuses.</returns>
    public async Task<Result<IReadOnlyList<DomainDiskUsageDto>>> HandleAsync(GetDiskUsageByDomainsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.Domains.Count == 0)
        {
            return Result<IReadOnlyList<DomainDiskUsageDto>>.Success([]);
        }

        var domains = query.Domains.Select(d => d.ToLowerInvariant()).Distinct().ToList();

        // The server reports usage per account, keyed by its own name and the account's domain
        // (domain compared case-insensitively, as the usage sync stores it).
        var samples = await _context.DiskUsageStats
            .AsNoTracking()
            .Where(u => domains.Contains(u.Domain.ToLower()))
            .ToListAsync(ct);

        IReadOnlyList<DomainDiskUsageDto> newest = samples
            .GroupBy(u => (u.ServerName, Domain: u.Domain.ToLowerInvariant()))
            .Select(g => g.OrderByDescending(u => u.UpdatedAt).First())
            .Select(u => new DomainDiskUsageDto(
                u.ServerName, u.Domain.ToLowerInvariant(), u.DiskUsageMb, u.DiskLimitMb, u.BwUsageMb, u.BwLimitMb, u.UpdatedAt))
            .ToList();

        return Result<IReadOnlyList<DomainDiskUsageDto>>.Success(newest);
    }
}
