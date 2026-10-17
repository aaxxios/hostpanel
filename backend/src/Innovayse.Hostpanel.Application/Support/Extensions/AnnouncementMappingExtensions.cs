using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Domain.Support;

namespace Innovayse.Hostpanel.Application.Support.Extensions;

/// <summary>
/// Maps <see cref="Announcement"/> to <see cref="AnnouncementDto"/>. Every query that returns this DTO projects
/// through this one mapping, so a field added to the DTO is added once rather than in several
/// handlers that quietly drift apart.
/// </summary>
public static class AnnouncementMappingExtensions
{
    /// <summary>Projects the query to <see cref="AnnouncementDto"/> rows, translated into the SQL select list.</summary>
    /// <param name="source">The query to project.</param>
    /// <returns>The query projected to <see cref="AnnouncementDto"/>.</returns>
    public static IQueryable<AnnouncementDto> SelectDto(this IQueryable<Announcement> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(a => new AnnouncementDto(a.Id, a.Title, a.Content, a.IsPublished, a.CreatedAt));
    }
}
