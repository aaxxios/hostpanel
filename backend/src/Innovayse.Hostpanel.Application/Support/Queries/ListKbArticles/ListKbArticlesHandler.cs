using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Application.Support.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Queries.ListKbArticles;

/// <summary>Returns knowledge base articles, optionally filtered to published only.</summary>
public sealed class ListKbArticlesHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public ListKbArticlesHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The listing filter.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with the matching articles. This use case answers no refusal code.</returns>
    public async Task<Result<IReadOnlyList<KbArticleDto>>> HandleAsync(ListKbArticlesQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var articles = await _context.KbArticles
            .AsNoTracking()
            .Where(a => !query.PublishedOnly || a.IsPublished)
            .SelectDto()
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<KbArticleDto>>(articles);
    }
}
