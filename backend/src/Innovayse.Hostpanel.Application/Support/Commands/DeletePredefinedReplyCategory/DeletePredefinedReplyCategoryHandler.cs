using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Commands.DeletePredefinedReplyCategory;

/// <summary>Deletes a predefined reply category.</summary>
public sealed class DeletePredefinedReplyCategoryHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public DeletePredefinedReplyCategoryHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The category to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, <c>PREDEFINED_REPLY_CATEGORY_NOT_FOUND</c> when the id names no category, or
    /// <c>PREDEFINED_REPLY_CATEGORY_IN_USE</c> when it still has sub-categories or replies.
    /// </returns>
    public async Task<Result> HandleAsync(DeletePredefinedReplyCategoryCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var category = await _context.PredefinedReplyCategories.FirstOrDefaultAsync(c => c.Id == cmd.Id, ct);
        if (category is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.PREDEFINED_REPLY_CATEGORY_NOT_FOUND);
        }

        if (await _context.PredefinedReplyCategories.AnyAsync(c => c.ParentCategoryId == cmd.Id, ct)
            || await _context.PredefinedReplies.AnyAsync(r => r.CategoryId == cmd.Id, ct))
        {
            return Result.FailureCode(HostpanelErrorCodes.PREDEFINED_REPLY_CATEGORY_IN_USE);
        }

        _context.PredefinedReplyCategories.Remove(category);
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
