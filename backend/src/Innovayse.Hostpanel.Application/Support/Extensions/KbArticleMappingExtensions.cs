using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Domain.Support;

namespace Innovayse.Hostpanel.Application.Support.Extensions;

/// <summary>
/// Maps <see cref="KbArticle"/> to <see cref="KbArticleDto"/>, <see cref="PublishedKbArticleDto"/> and
/// <see cref="KbArticleSummaryDto"/>. Every
/// query that returns one of these DTOs projects through this one mapping, so a field added to a DTO is
/// added once rather than in several handlers that quietly drift apart.
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

    /// <summary>
    /// Projects the query to <see cref="PublishedKbArticleDto"/> rows, translated into the SQL select list.
    /// The caller filters to published rows first.
    /// </summary>
    /// <param name="source">The query to project.</param>
    /// <returns>The query projected to <see cref="PublishedKbArticleDto"/>.</returns>
    public static IQueryable<PublishedKbArticleDto> SelectPublishedDto(this IQueryable<KbArticle> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(a => new PublishedKbArticleDto(a.Id, a.Title, a.Content, a.Category));
    }

    /// <summary>
    /// Projects the query to <see cref="KbArticleSummarySource"/> rows. The database cuts the body to its
    /// opening (<c>SUBSTRING</c>), so the rest of a long body never leaves the server.
    /// </summary>
    /// <param name="source">The query to project.</param>
    /// <returns>The query projected to <see cref="KbArticleSummarySource"/>.</returns>
    public static IQueryable<KbArticleSummarySource> SelectSummarySource(this IQueryable<KbArticle> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(a => new KbArticleSummarySource(
            a.Id, a.Title, a.Content.Substring(0, KbArticleExcerpt.SourceLength), a.Category));
    }

    /// <summary>Builds the listing row, with its plain-text excerpt, from what the database returned.</summary>
    /// <param name="source">The row as read.</param>
    /// <returns>The summary DTO.</returns>
    public static KbArticleSummaryDto ToSummaryDto(this KbArticleSummarySource source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new KbArticleSummaryDto(source.Id, source.Title, KbArticleExcerpt.From(source.Lead), source.Category);
    }
}
