using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Projects.Common;
using Innovayse.Hostpanel.Application.Projects.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Projects.Queries.ListProjects;

/// <summary>Loads every project with its translations and maps them to <see cref="ProjectAdminDto"/>.</summary>
public sealed class ListProjectsHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public ListProjectsHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the query.
    /// </summary>
    /// <param name="query">The query; it carries no input.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Every project, ordered by sort order ascending. Never refuses.</returns>
    public async Task<Result<IReadOnlyList<ProjectAdminDto>>> HandleAsync(ListProjectsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var rows = await _context.Projects.AsNoTracking().OrderBy(x => x.SortOrder).ThenBy(x => x.Id).ToListAsync(ct);
        return Result<IReadOnlyList<ProjectAdminDto>>.Success(rows.Select(x => x.ToAdminDto()).ToList());
    }
}
