using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Projects.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Projects.Commands.UpdateProject;

/// <summary>
/// Loads an existing project, applies its new field values, replaces its translations, and persists
/// the result.
/// </summary>
public sealed class UpdateProjectHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public UpdateProjectHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The project's new fields and translations.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>PROJECT_NOT_FOUND</c> when the id names no project, or <c>PROJECT_SLUG_EXISTS</c> when another project already uses the slug.</returns>
    public async Task<Result> HandleAsync(UpdateProjectCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var entity = await _context.Projects.FirstOrDefaultAsync(x => x.Id == cmd.Id, ct);
        if (entity is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.PROJECT_NOT_FOUND);
        }

        if (await _context.Projects.AnyAsync(p => p.Slug == cmd.Slug && p.Id != cmd.Id, ct))
        {
            return Result.FailureCode(HostpanelErrorCodes.PROJECT_SLUG_EXISTS);
        }

        entity.Update(cmd.Slug, cmd.Url, cmd.ImageUrls, cmd.SortOrder, cmd.IsActive);
        entity.ReplaceTranslations(cmd.Translations.ToContent());

        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Another writer took the slug between the check above and this save, and the unique index
            // refused. Asked again rather than assumed: any other failed save is not this conflict and
            // must still surface as the error it is.
            _context.DiscardPendingChanges();
            if (await _context.Projects.AnyAsync(p => p.Slug == cmd.Slug && p.Id != cmd.Id, ct))
            {
                return Result.FailureCode(HostpanelErrorCodes.PROJECT_SLUG_EXISTS);
            }

            throw;
        }

        return Result.Success();
    }
}
