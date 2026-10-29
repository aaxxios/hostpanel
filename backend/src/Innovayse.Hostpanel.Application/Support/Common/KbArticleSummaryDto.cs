namespace Innovayse.Hostpanel.Application.Support.Common;

/// <summary>
/// One row of the published knowledge base listing: what a list shows, without the article body,
/// so a page of results never carries every article's full HTML.
/// </summary>
/// <param name="Id">Article primary key.</param>
/// <param name="Title">The article title.</param>
/// <param name="Excerpt">The opening of the body as plain text, at most <see cref="KbArticleExcerpt.MaxLength"/> characters.</param>
/// <param name="Category">The category this article belongs to.</param>
public sealed record KbArticleSummaryDto(int Id, string Title, string Excerpt, string Category);
