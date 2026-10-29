namespace Innovayse.Hostpanel.Application.Support.Common;

/// <summary>
/// What the database returns for one row of the article listing: the fields of the summary and only the
/// opening of the body, from which the handler builds the excerpt. The full body is never read.
/// </summary>
/// <param name="Id">The article identifier.</param>
/// <param name="Title">The article title.</param>
/// <param name="Lead">The first <see cref="KbArticleExcerpt.SourceLength"/> characters of the body, markup included.</param>
/// <param name="Category">The category name.</param>
public sealed record KbArticleSummarySource(int Id, string Title, string Lead, string Category);
