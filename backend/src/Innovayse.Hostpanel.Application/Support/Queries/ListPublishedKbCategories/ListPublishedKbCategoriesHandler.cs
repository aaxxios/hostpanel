using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Queries.ListPublishedKbCategories;

/// <summary>
/// Returns the categories of the published knowledge base articles with their counts, grouped and counted in the
/// database so no article body is read.
/// </summary>
public sealed class ListPublishedKbCategoriesHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public ListPublishedKbCategoriesHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The query; it carries no input.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success with one entry per category name of a published article, ordered by name; names that differ only by
    /// letter case are one category, shown under the first of their spellings in sort order. An article with a
    /// blank category is left out, and a category holding only drafts does not appear. This use case answers no
    /// refusal code.
    /// </returns>
    public async Task<Result<IReadOnlyList<PublishedKbCategoryDto>>> HandleAsync(
        ListPublishedKbCategoriesQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var categories = await _context.KbArticles
            .AsNoTracking()
            .Where(a => a.IsPublished && a.Category.Trim() != string.Empty)
            .GroupBy(a => a.Category.Trim().ToLower())
            .OrderBy(g => g.Key)
            .Select(g => new PublishedKbCategoryDto(g.Min(a => a.Category.Trim())!, g.Count()))
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<PublishedKbCategoryDto>>(categories);
    }
}
