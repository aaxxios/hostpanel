namespace Innovayse.Hostpanel.Application.Support.Queries.GetPublishedKbArticle;

/// <summary>Query to retrieve a knowledge base article that is published.</summary>
/// <remarks>
/// The public knowledge base route dispatches this rather than <c>GetKbArticleQuery</c>, which
/// reads a row by id and never looks at its published state: on an anonymous route that would put
/// every unpublished draft one guessed id away from any visitor. <c>GetKbArticleQuery</c> is the
/// admin read that may return a draft.
/// </remarks>
/// <param name="Id">The article primary key.</param>
public sealed record GetPublishedKbArticleQuery(int Id);
