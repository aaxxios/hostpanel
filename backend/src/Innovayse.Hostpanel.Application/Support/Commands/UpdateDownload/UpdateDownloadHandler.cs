using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Commands.UpdateDownload;

/// <summary>Updates an existing download entry.</summary>
public sealed class UpdateDownloadHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public UpdateDownloadHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The download to update and its new details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, <c>DOWNLOAD_NOT_FOUND</c> when the id names no download, or
    /// <c>DOWNLOAD_CATEGORY_NOT_FOUND</c> when the category names no category.
    /// </returns>
    public async Task<Result> HandleAsync(UpdateDownloadCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var download = await _context.Downloads.FirstOrDefaultAsync(d => d.Id == cmd.Id, ct);
        if (download is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.DOWNLOAD_NOT_FOUND);
        }

        if (!await _context.DownloadCategories.AnyAsync(c => c.Id == cmd.CategoryId, ct))
        {
            return Result.FailureCode(HostpanelErrorCodes.DOWNLOAD_CATEGORY_NOT_FOUND);
        }

        download.Update(
            cmd.Title,
            cmd.Description,
            cmd.Type,
            cmd.Filename,
            cmd.CategoryId,
            cmd.ClientsOnly,
            cmd.ProductDownload,
            cmd.IsHidden);

        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
