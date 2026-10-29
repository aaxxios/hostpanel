namespace Innovayse.Hostpanel.Application.Support.Queries.ListPublishedKbArticles;

/// <summary>Query for one page of the published knowledge base articles, optionally narrowed by category and search term.</summary>
/// <param name="Category">Category name to restrict to (matched case-insensitively), or null for every category.</param>
/// <param name="Search">Free-text term matched case-insensitively against title and content, or null to list without searching.</param>
/// <param name="Page">1-based page number.</param>
/// <param name="PageSize">Items per page, 1 to 100.</param>
public sealed record ListPublishedKbArticlesQuery(
    string? Category = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 20);
