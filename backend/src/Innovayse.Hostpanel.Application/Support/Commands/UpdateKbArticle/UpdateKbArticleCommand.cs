namespace Innovayse.Hostpanel.Application.Support.Commands.UpdateKbArticle;

/// <summary>Command to update an existing knowledge base article.</summary>
/// <param name="Id">The article primary key; nullable so the route id can overlay the body.</param>
/// <param name="Title">The new article title.</param>
/// <param name="Content">The new article body content.</param>
/// <param name="Category">The new category name.</param>
public sealed record UpdateKbArticleCommand(int? Id, string Title, string Content, string Category);
