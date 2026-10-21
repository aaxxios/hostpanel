using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Locations.Common;
using Innovayse.Hostpanel.Application.Locations.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Locations.Queries.ListLocations;

/// <summary>Loads every location with its translations and maps them to <see cref="LocationAdminDto"/>.</summary>
public sealed class ListLocationsHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public ListLocationsHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the query.
    /// </summary>
    /// <param name="query">The query; it carries no input.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Every location, ordered by sort order ascending. Never refuses.</returns>
    public async Task<Result<IReadOnlyList<LocationAdminDto>>> HandleAsync(ListLocationsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var rows = await _context.ServerLocations.AsNoTracking().OrderBy(x => x.SortOrder).ThenBy(x => x.Id).ToListAsync(ct);
        return Result<IReadOnlyList<LocationAdminDto>>.Success(rows.Select(x => x.ToAdminDto()).ToList());
    }
}
