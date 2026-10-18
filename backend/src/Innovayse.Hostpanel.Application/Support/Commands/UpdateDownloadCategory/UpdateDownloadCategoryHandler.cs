using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Commands.UpdateDownloadCategory;

/// <summary>Updates an existing download category.</summary>
public sealed class UpdateDownloadCategoryHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public UpdateDownloadCategoryHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The category to update and its new details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>DOWNLOAD_CATEGORY_NOT_FOUND</c> when the id names no category.</returns>
    public async Task<Result> HandleAsync(UpdateDownloadCategoryCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var category = await _context.DownloadCategories.FirstOrDefaultAsync(c => c.Id == cmd.Id, ct);
        if (category is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.DOWNLOAD_CATEGORY_NOT_FOUND);
        }

        category.Update(cmd.Name, cmd.Description, cmd.IsHidden, cmd.ParentCategoryId);
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
