using Innovayse.Hostpanel.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Extensions;

/// <summary>
/// Answers whether the department an id taken from input points at exists. A Support handler that
/// stores such an id checks it here before saving, so a missing department is refused with its
/// <c>DEPARTMENT_NOT_FOUND</c> code instead of surfacing as a failed save.
/// </summary>
public static class DepartmentExistenceExtensions
{
    /// <summary>Whether a support department with this id exists.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="departmentId">The id to look for.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><see langword="true"/> when the department exists.</returns>
    public static Task<bool> DepartmentExistsAsync(this IHostpanelDbContext context, int departmentId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.Departments.AsNoTracking().AnyAsync(x => x.Id == departmentId, ct);
    }
}
