using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Queries.ListDownloadCategories;

/// <summary>Returns all download categories with download counts per category.</summary>
public sealed class ListDownloadCategoriesHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public ListDownloadCategoriesHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The query; it carries no input.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Every category, by name, with its download count. Never refuses.</returns>
    public async Task<Result<IReadOnlyList<DownloadCategoryDto>>> HandleAsync(
        ListDownloadCategoriesQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var categories = await _context.DownloadCategories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new DownloadCategoryDto(
                c.Id,
                c.Name,
                c.Description,
                c.IsHidden,
                c.ParentCategoryId,
                _context.Downloads.Count(d => d.CategoryId == c.Id)))
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<DownloadCategoryDto>>(categories);
    }
}
