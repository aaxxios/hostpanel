using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Projects.Extensions;
using Innovayse.Hostpanel.Domain.Projects;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Projects.Commands.CreateProject;

/// <summary>
/// Creates a new <see cref="Project"/> aggregate, applies every translation the command carries, and
/// persists the result.
/// </summary>
public sealed class CreateProjectHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public CreateProjectHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The project and its translations.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The newly created project's id, or <c>PROJECT_SLUG_EXISTS</c> when another project already uses the slug.</returns>
    public async Task<Result<int>> HandleAsync(CreateProjectCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        if (await _context.Projects.AnyAsync(p => p.Slug == cmd.Slug, ct))
        {
            return Result<int>.FailureCode(HostpanelErrorCodes.PROJECT_SLUG_EXISTS);
        }

        var entity = Project.Create(cmd.Slug, cmd.Url, cmd.ImageUrls, cmd.SortOrder, cmd.IsActive);
        entity.ApplyTranslations(cmd.Translations);

        _context.Projects.Add(entity);
        await _context.SaveChangesAsync(ct);
        return Result<int>.Success(entity.Id);
    }
}
