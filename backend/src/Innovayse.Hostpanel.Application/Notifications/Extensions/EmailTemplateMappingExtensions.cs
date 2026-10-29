using Innovayse.Hostpanel.Application.Notifications.Common;
using Innovayse.Hostpanel.Domain.Notifications;

namespace Innovayse.Hostpanel.Application.Notifications.Extensions;

/// <summary>
/// Maps <see cref="EmailTemplate"/> to <see cref="EmailTemplateDto"/>. Every query that returns this DTO projects
/// through this one mapping, so a field added to the DTO is added once rather than in several
/// handlers that quietly drift apart.
/// </summary>
public static class EmailTemplateMappingExtensions
{
    /// <summary>Projects the query to <see cref="EmailTemplateDto"/> rows, translated into the SQL select list.</summary>
    /// <param name="source">The query to project.</param>
    /// <returns>The query projected to <see cref="EmailTemplateDto"/>.</returns>
    public static IQueryable<EmailTemplateDto> SelectDto(this IQueryable<EmailTemplate> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(t => new EmailTemplateDto(t.Id, t.Slug, t.Language, t.Subject, t.Body, t.Description, t.IsActive));
    }
}
