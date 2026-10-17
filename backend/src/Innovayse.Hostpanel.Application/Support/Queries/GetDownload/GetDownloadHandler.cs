using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Queries.GetDownload;

/// <summary>Loads a single download and maps it to <see cref="DownloadDto"/>.</summary>
public sealed class GetDownloadHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetDownloadHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The download to load.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The matching download, or <c>DOWNLOAD_NOT_FOUND</c> when the id names none.</returns>
    public async Task<Result<DownloadDto>> HandleAsync(GetDownloadQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var download = await _context.Downloads.AsNoTracking().FirstOrDefaultAsync(d => d.Id == query.Id, ct);
        if (download is null)
        {
            return Result<DownloadDto>.FailureCode(HostpanelErrorCodes.DOWNLOAD_NOT_FOUND);
        }

        var categoryName = await _context.DownloadCategories
            .AsNoTracking()
            .Where(c => c.Id == download.CategoryId)
            .Select(c => c.Name)
            .FirstOrDefaultAsync(ct);

        var dto = new DownloadDto(
            download.Id,
            download.Title,
            download.Description,
            download.Type,
            download.Filename,
            download.CategoryId,
            categoryName,
            download.DownloadCount,
            download.ClientsOnly,
            download.ProductDownload,
            download.IsHidden,
            download.CreatedAt);

        return Result.Success(dto);
    }
}
