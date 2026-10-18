using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Domain.Support;

namespace Innovayse.Hostpanel.Application.Support.Extensions;

/// <summary>
/// Maps <see cref="Department"/> to <see cref="DepartmentDto"/>. Every query that returns this DTO projects
/// through this one mapping, so a field added to the DTO is added once rather than in several
/// handlers that quietly drift apart.
/// </summary>
public static class DepartmentMappingExtensions
{
    /// <summary>Projects the query to <see cref="DepartmentDto"/> rows, translated into the SQL select list.</summary>
    /// <param name="source">The query to project.</param>
    /// <returns>The query projected to <see cref="DepartmentDto"/>.</returns>
    public static IQueryable<DepartmentDto> SelectDto(this IQueryable<Department> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(d => new DepartmentDto(d.Id, d.Name, d.Email));
    }
}
