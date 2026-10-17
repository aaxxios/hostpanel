using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Application.Support.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Queries.ListDepartments;

/// <summary>
/// Returns all support departments mapped to <see cref="DepartmentDto"/>, for the staff ticket
/// screens' department filter.
/// </summary>
public sealed class ListDepartmentsHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public ListDepartmentsHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The query; it carries no input.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Every department, by name. Never refuses.</returns>
    public async Task<Result<IReadOnlyList<DepartmentDto>>> HandleAsync(ListDepartmentsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var departments = await _context.Departments
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .SelectDto()
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<DepartmentDto>>(departments);
    }
}
