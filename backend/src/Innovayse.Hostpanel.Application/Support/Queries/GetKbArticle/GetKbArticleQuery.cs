namespace Innovayse.Hostpanel.Application.Support.Queries.GetKbArticle;

/// <summary>Query to retrieve a single knowledge base article by ID, regardless of published state.</summary>
/// <param name="Id">The article primary key.</param>
public sealed record GetKbArticleQuery(int Id);
