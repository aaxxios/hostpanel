using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Locations.Commands.DeleteLocation;

/// <summary>Deletes a location permanently from the persistence store; its translations go with it.</summary>
public sealed class DeleteLocationHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public DeleteLocationHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The location to remove.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>LOCATION_NOT_FOUND</c> when the id names no location.</returns>
    public async Task<Result> HandleAsync(DeleteLocationCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var entity = await _context.ServerLocations.FirstOrDefaultAsync(x => x.Id == cmd.Id, ct);
        if (entity is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.LOCATION_NOT_FOUND);
        }

        _context.ServerLocations.Remove(entity);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
