namespace Innovayse.Hostpanel.Application.Support.Commands.CreateKbArticle;

/// <summary>Command to create a new knowledge base article.</summary>
/// <param name="Title">The article title.</param>
/// <param name="Content">The full article body content.</param>
/// <param name="Category">The category name for the article.</param>
/// <param name="IsPublished">Whether the article is visible to clients at once; omitted means a draft.</param>
public sealed record CreateKbArticleCommand(string Title, string Content, string Category, bool IsPublished = false);
