using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Extensions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Locations.Commands.UpdateLocationOrder;

/// <summary>
/// Applies the new sort order values for the ids listed in the command and persists the reordering in one
/// transaction. Locations whose ids are not in the command are left unchanged.
/// </summary>
public sealed class UpdateLocationOrderHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public UpdateLocationOrderHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The location id and new sort order pairs.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>LOCATION_NOT_FOUND</c> when an id names no row; nothing is moved then.</returns>
    public async Task<Result> HandleAsync(UpdateLocationOrderCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        if (!await _context.ServerLocations.ApplySortOrderAsync(cmd.Items, ct))
        {
            return Result.FailureCode(HostpanelErrorCodes.LOCATION_NOT_FOUND);
        }

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
