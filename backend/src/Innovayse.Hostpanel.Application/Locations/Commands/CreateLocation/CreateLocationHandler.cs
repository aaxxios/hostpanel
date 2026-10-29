using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Locations.Extensions;
using Innovayse.Hostpanel.Domain.Locations;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Locations.Commands.CreateLocation;

/// <summary>
/// Creates a new <see cref="ServerLocation"/> aggregate, applies every translation the command carries, and
/// persists the result.
/// </summary>
public sealed class CreateLocationHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public CreateLocationHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The location and its translations.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The newly created location's id. This use case refuses nothing.</returns>
    public async Task<Result<int>> HandleAsync(CreateLocationCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var entity = ServerLocation.Create(cmd.CountryCode, cmd.SortOrder, cmd.IsActive);
        entity.ReplaceTranslations(cmd.Translations.ToContent());

        _context.ServerLocations.Add(entity);
        await _context.SaveChangesAsync(ct);
        return Result<int>.Success(entity.Id);
    }
}
