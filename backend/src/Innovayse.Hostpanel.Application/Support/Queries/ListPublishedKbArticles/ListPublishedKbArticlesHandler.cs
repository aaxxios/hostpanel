using System.Globalization;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Application.Support.Extensions;
using Innovayse.Hostpanel.Shared.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Queries.ListPublishedKbArticles;

/// <summary>Returns one page of published knowledge base articles, filtered by category and search term in SQL; only the opening of each body is read, for the excerpt.</summary>
public sealed class ListPublishedKbArticlesHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public ListPublishedKbArticlesHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">Filters and paging.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with the page of article summaries (no bodies), ordered by id. This use case answers no refusal code.</returns>
    public async Task<Result<PaginatedList<KbArticleSummaryDto>>> HandleAsync(ListPublishedKbArticlesQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var articles = _context.KbArticles
            .AsNoTracking()
            .Where(a => a.IsPublished);

        var category = query.Category?.Trim().ToLower(CultureInfo.InvariantCulture);
        if (!string.IsNullOrEmpty(category))
        {
            articles = articles.Where(a => a.Category.ToLower() == category);
        }

        // EF.Functions.Like rather than the Npgsql-only ILike: Application does not reference the
        // Npgsql provider package, and lower-casing both sides gives the case-insensitive match.
        // The term is escaped so a typed %, _ or backslash matches itself instead of widening the search.
        var search = query.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            var term = LikePattern.Contains(search);
            articles = articles.Where(a =>
                EF.Functions.Like(a.Title.ToLower(), term, LikePattern.EscapeCharacter)
                || EF.Functions.Like(a.Content.ToLower(), term, LikePattern.EscapeCharacter));
        }

        var totalCount = await articles.CountAsync(ct);
        var rows = await articles
            .OrderBy(a => a.Id)
            .Paginate(query.Page, query.PageSize)
            .SelectSummarySource()
            .ToListAsync(ct);

        var items = rows.Select(r => r.ToSummaryDto()).ToList();

        return Result.Success(new PaginatedList<KbArticleSummaryDto>(items, totalCount, query.Page, query.PageSize));
    }
}
