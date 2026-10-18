using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Commands.UpdateKbCategory;

/// <summary>Updates an existing knowledge base category.</summary>
public sealed class UpdateKbCategoryHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public UpdateKbCategoryHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="command">The category to rewrite.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>KBCATEGORY_NOT_FOUND</c> when the id names no category.</returns>
    public async Task<Result> HandleAsync(UpdateKbCategoryCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var category = await _context.KbCategories.FirstOrDefaultAsync(c => c.Id == command.Id, ct);
        if (category is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.KBCATEGORY_NOT_FOUND);
        }

        category.Update(command.Name, command.Description, command.IsHidden);
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
