using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Support;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Commands.CreateDownloadCategory;

/// <summary>Creates a new download category.</summary>
public sealed class CreateDownloadCategoryHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public CreateDownloadCategoryHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The category's details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success with the new category's id, or <c>DOWNLOAD_CATEGORY_NOT_FOUND</c> when the parent
    /// category names no category.
    /// </returns>
    public async Task<Result<int>> HandleAsync(CreateDownloadCategoryCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        if (cmd.ParentCategoryId is { } parentId
            && !await _context.DownloadCategories.AnyAsync(c => c.Id == parentId, ct))
        {
            return Result<int>.FailureCode(HostpanelErrorCodes.DOWNLOAD_CATEGORY_NOT_FOUND);
        }

        var category = DownloadCategory.Create(cmd.Name, cmd.Description, cmd.IsHidden, cmd.ParentCategoryId);

        _context.DownloadCategories.Add(category);
        await _context.SaveChangesAsync(ct);

        return Result.Success(category.Id);
    }
}
