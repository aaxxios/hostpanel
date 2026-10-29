using Innovayse.Hostpanel.Application.Notifications.Common;
using Innovayse.Hostpanel.Domain.Notifications;

namespace Innovayse.Hostpanel.Application.Notifications.Extensions;

/// <summary>
/// Maps <see cref="EmailLog"/> to <see cref="EmailLogDto"/>. Every query that returns this DTO projects
/// through this one mapping, so a field added to the DTO is added once rather than in several
/// handlers that quietly drift apart.
/// </summary>
public static class EmailLogMappingExtensions
{
    /// <summary>Projects the query to <see cref="EmailLogDto"/> rows, translated into the SQL select list.</summary>
    /// <param name="source">The query to project.</param>
    /// <returns>The query projected to <see cref="EmailLogDto"/>.</returns>
    public static IQueryable<EmailLogDto> SelectDto(this IQueryable<EmailLog> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(l => new EmailLogDto(l.Id, l.To, l.Subject, l.SentAt, l.Success, l.Failure));
    }
}
