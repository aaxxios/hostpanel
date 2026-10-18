using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Queries.ListPredefinedReplyCategories;

/// <summary>Returns all predefined reply categories with their reply counts.</summary>
public sealed class ListPredefinedReplyCategoriesHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public ListPredefinedReplyCategoriesHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The query; it carries no input.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Every category, by name, with its reply count. Never refuses.</returns>
    public async Task<Result<IReadOnlyList<PredefinedReplyCategoryDto>>> HandleAsync(
        ListPredefinedReplyCategoriesQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var categories = await _context.PredefinedReplyCategories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new PredefinedReplyCategoryDto(
                c.Id,
                c.Name,
                c.ParentCategoryId,
                _context.PredefinedReplies.Count(r => r.CategoryId == c.Id)))
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<PredefinedReplyCategoryDto>>(categories);
    }
}
