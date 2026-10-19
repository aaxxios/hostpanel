using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Projects.Common;
using Innovayse.Hostpanel.Application.Projects.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Projects.Queries.GetProject;

/// <summary>Loads a single project with every translation and maps it to <see cref="ProjectAdminDto"/>.</summary>
public sealed class GetProjectHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public GetProjectHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the query.
    /// </summary>
    /// <param name="query">The project id to retrieve.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The admin DTO, or <c>PROJECT_NOT_FOUND</c> when the id names no project.</returns>
    public async Task<Result<ProjectAdminDto>> HandleAsync(GetProjectQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var entity = await _context.Projects.AsNoTracking().FirstOrDefaultAsync(x => x.Id == query.Id, ct);
        if (entity is null)
        {
            return Result<ProjectAdminDto>.FailureCode(HostpanelErrorCodes.PROJECT_NOT_FOUND);
        }

        return Result<ProjectAdminDto>.Success(entity.ToAdminDto());
    }
}
