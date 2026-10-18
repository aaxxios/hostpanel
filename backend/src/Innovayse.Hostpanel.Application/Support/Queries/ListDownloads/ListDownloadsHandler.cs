using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Queries.ListDownloads;

/// <summary>Returns downloads optionally filtered by category, with resolved category names.</summary>
public sealed class ListDownloadsHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public ListDownloadsHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The optional category filter.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Every matching download, newest first. Never refuses.</returns>
    public async Task<Result<IReadOnlyList<DownloadDto>>> HandleAsync(ListDownloadsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        // Ordered before the join is projected into the DTO: EF cannot translate an ORDER BY
        // applied to a property read off a constructed record, only to a column.
        var downloads = await _context.Downloads
            .AsNoTracking()
            .Where(d => query.CategoryId == null || d.CategoryId == query.CategoryId.Value)
            .OrderByDescending(d => d.CreatedAt)
            .GroupJoin(
                _context.DownloadCategories.AsNoTracking(),
                d => d.CategoryId,
                c => c.Id,
                (d, categories) => new { Download = d, Categories = categories })
            .SelectMany(
                x => x.Categories.DefaultIfEmpty(),
                (x, category) => new DownloadDto(
                    x.Download.Id,
                    x.Download.Title,
                    x.Download.Description,
                    x.Download.Type,
                    x.Download.Filename,
                    x.Download.CategoryId,
                    category != null ? category.Name : null,
                    x.Download.DownloadCount,
                    x.Download.ClientsOnly,
                    x.Download.ProductDownload,
                    x.Download.IsHidden,
                    x.Download.CreatedAt))
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<DownloadDto>>(downloads);
    }
}
