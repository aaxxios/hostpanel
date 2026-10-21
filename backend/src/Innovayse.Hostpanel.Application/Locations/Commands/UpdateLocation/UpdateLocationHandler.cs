using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Locations.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Locations.Commands.UpdateLocation;

/// <summary>
/// Loads an existing location, applies its new field values, replaces its translations, and persists
/// the result.
/// </summary>
public sealed class UpdateLocationHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public UpdateLocationHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The location's new fields and translations.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>LOCATION_NOT_FOUND</c> when the id names no location.</returns>
    public async Task<Result> HandleAsync(UpdateLocationCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var entity = await _context.ServerLocations.FirstOrDefaultAsync(x => x.Id == cmd.Id, ct);
        if (entity is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.LOCATION_NOT_FOUND);
        }

        entity.Update(cmd.CountryCode, cmd.SortOrder, cmd.IsActive);
        entity.ApplyTranslations(cmd.Translations);

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
