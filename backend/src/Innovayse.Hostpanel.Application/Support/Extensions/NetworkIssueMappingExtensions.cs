using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Domain.Support;

namespace Innovayse.Hostpanel.Application.Support.Extensions;

/// <summary>
/// Maps <see cref="NetworkIssue"/> to <see cref="NetworkIssueDto"/>. Every query that returns this DTO projects
/// through this one mapping, so a field added to the DTO is added once rather than in several
/// handlers that quietly drift apart.
/// </summary>
public static class NetworkIssueMappingExtensions
{
    /// <summary>Projects the query to <see cref="NetworkIssueDto"/> rows, translated into the SQL select list.</summary>
    /// <param name="source">The query to project.</param>
    /// <returns>The query projected to <see cref="NetworkIssueDto"/>.</returns>
    public static IQueryable<NetworkIssueDto> SelectDto(this IQueryable<NetworkIssue> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(n => new NetworkIssueDto(
            n.Id,
            n.Title,
            n.Type,
            n.Server,
            n.Priority,
            n.Status,
            n.StartDate,
            n.EndDate,
            n.Description,
            n.CreatedAt));
    }
}
