using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Reports.Queries.ClientsByCity;

/// <summary>Handles <see cref="ClientsByCityQuery"/>.</summary>
public sealed class ClientsByCityHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public ClientsByCityHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The query. Carries no filters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Client counts grouped by city and country, largest first. Never refuses.</returns>
    public async Task<Result<IReadOnlyList<ClientsByCityDto>>> HandleAsync(ClientsByCityQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var grouped = await _context.Clients
            .AsNoTracking()
            .Where(c => c.City != null && c.City != string.Empty && c.Country != null && c.Country != string.Empty)
            .GroupBy(c => new { c.City, c.Country })
            .Select(g => new { g.Key.City, g.Key.Country, Count = g.Count() })
            .ToListAsync(ct);

        IReadOnlyList<ClientsByCityDto> rows = grouped
            .OrderByDescending(x => x.Count)
            .Select(x => new ClientsByCityDto(x.City!, x.Country!, x.Count))
            .ToList();

        return Result<IReadOnlyList<ClientsByCityDto>>.Success(rows);
    }
}
