using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Commands.DeleteDownloadCategory;

/// <summary>Deletes a download category.</summary>
public sealed class DeleteDownloadCategoryHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public DeleteDownloadCategoryHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The category to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, <c>DOWNLOAD_CATEGORY_NOT_FOUND</c> when the id names no category, or
    /// <c>DOWNLOAD_CATEGORY_IN_USE</c> when it still has sub-categories or downloads.
    /// </returns>
    public async Task<Result> HandleAsync(DeleteDownloadCategoryCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var category = await _context.DownloadCategories.FirstOrDefaultAsync(c => c.Id == cmd.Id, ct);
        if (category is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.DOWNLOAD_CATEGORY_NOT_FOUND);
        }

        if (await _context.DownloadCategories.AnyAsync(c => c.ParentCategoryId == cmd.Id, ct)
            || await _context.Downloads.AnyAsync(d => d.CategoryId == cmd.Id, ct))
        {
            return Result.FailureCode(HostpanelErrorCodes.DOWNLOAD_CATEGORY_IN_USE);
        }

        _context.DownloadCategories.Remove(category);
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
