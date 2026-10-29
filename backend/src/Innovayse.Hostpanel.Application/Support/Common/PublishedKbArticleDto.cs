namespace Innovayse.Hostpanel.Application.Support.Common;

/// <summary>
/// The knowledge base article projection a client is allowed to see.
/// </summary>
/// <remarks>
/// Deliberately not <see cref="KbArticleDto"/>. That record carries <c>IsPublished</c>, an
/// editorial fact about the row rather than help for a reader. Only published rows reach this
/// type, so the flag has nothing left to say and is left out rather than pinned to a constant.
/// </remarks>
/// <param name="Id">Article primary key.</param>
/// <param name="Title">The article title.</param>
/// <param name="Content">The full article body content.</param>
/// <param name="Category">The category this article belongs to.</param>
public sealed record PublishedKbArticleDto(int Id, string Title, string Content, string Category);
