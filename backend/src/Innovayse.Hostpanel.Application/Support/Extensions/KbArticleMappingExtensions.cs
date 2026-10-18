using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Domain.Support;

namespace Innovayse.Hostpanel.Application.Support.Extensions;

/// <summary>
/// Maps <see cref="KbArticle"/> to <see cref="KbArticleDto"/>. Every query that returns this DTO projects
/// through this one mapping, so a field added to the DTO is added once rather than in several
/// handlers that quietly drift apart.
/// </summary>
public static class KbArticleMappingExtensions
{
    /// <summary>Projects the query to <see cref="KbArticleDto"/> rows, translated into the SQL select list.</summary>
    /// <param name="source">The query to project.</param>
    /// <returns>The query projected to <see cref="KbArticleDto"/>.</returns>
    public static IQueryable<KbArticleDto> SelectDto(this IQueryable<KbArticle> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(a => new KbArticleDto(a.Id, a.Title, a.Content, a.Category, a.IsPublished));
    }
}
