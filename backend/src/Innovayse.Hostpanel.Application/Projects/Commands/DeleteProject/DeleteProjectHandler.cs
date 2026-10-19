using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Projects.Commands.DeleteProject;

/// <summary>Deletes a project permanently from the persistence store; its translations go with it.</summary>
public sealed class DeleteProjectHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public DeleteProjectHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The project to remove.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>PROJECT_NOT_FOUND</c> when the id names no project.</returns>
    public async Task<Result> HandleAsync(DeleteProjectCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var entity = await _context.Projects.FirstOrDefaultAsync(x => x.Id == cmd.Id, ct);
        if (entity is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.PROJECT_NOT_FOUND);
        }

        _context.Projects.Remove(entity);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
