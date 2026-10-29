using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Queries.ListKbCategories;

/// <summary>Returns all knowledge base categories with article counts.</summary>
public sealed class ListKbCategoriesHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public ListKbCategoriesHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The list categories query.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with every category. This use case answers no refusal code.</returns>
    public async Task<Result<IReadOnlyList<KbCategoryDto>>> HandleAsync(ListKbCategoriesQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var categories = await _context.KbCategories.AsNoTracking().ToListAsync(ct);
        var articles = await _context.KbArticles.AsNoTracking().ToListAsync(ct);

        var countsByCategory = articles
            .GroupBy(a => a.Category)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

        var items = categories
            .Select(c => new KbCategoryDto(
                c.Id,
                c.Name,
                c.Description,
                c.IsHidden,
                c.ParentCategoryId,
                countsByCategory.GetValueOrDefault(c.Name, 0)))
            .ToList();

        return Result.Success<IReadOnlyList<KbCategoryDto>>(items);
    }
}
