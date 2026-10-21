using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Locations.Common;
using Innovayse.Hostpanel.Application.Locations.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Locations.Queries.GetLocation;

/// <summary>Loads a single location with every translation and maps it to <see cref="LocationAdminDto"/>.</summary>
public sealed class GetLocationHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public GetLocationHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the query.
    /// </summary>
    /// <param name="query">The location id to retrieve.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The admin DTO, or <c>LOCATION_NOT_FOUND</c> when the id names no location.</returns>
    public async Task<Result<LocationAdminDto>> HandleAsync(GetLocationQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var entity = await _context.ServerLocations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == query.Id, ct);
        if (entity is null)
        {
            return Result<LocationAdminDto>.FailureCode(HostpanelErrorCodes.LOCATION_NOT_FOUND);
        }

        return Result<LocationAdminDto>.Success(entity.ToAdminDto());
    }
}
