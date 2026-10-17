using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Application.Support.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Queries.GetDepartments;

/// <summary>
/// Returns all support departments mapped to <see cref="DepartmentDto"/>. Public read behind the
/// ticket form's department picker: a visitor with no account has to see this list before they
/// have any admin context, so it never refuses.
/// </summary>
public sealed class GetDepartmentsHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetDepartmentsHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The query; it carries no input.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Every department, by name. Never refuses.</returns>
    public async Task<Result<IReadOnlyList<DepartmentDto>>> HandleAsync(GetDepartmentsQuery query, CancellationToken ct)
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
