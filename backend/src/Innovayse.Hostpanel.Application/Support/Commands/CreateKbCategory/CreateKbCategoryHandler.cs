using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Support;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Commands.CreateKbCategory;

/// <summary>Creates a knowledge base category.</summary>
public sealed class CreateKbCategoryHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public CreateKbCategoryHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="command">The category to create.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The new category's id, or <c>KBCATEGORY_NOT_FOUND</c> when the parent category names no category.
    /// </returns>
    public async Task<Result<int>> HandleAsync(CreateKbCategoryCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.ParentCategoryId is { } parentId
            && !await _context.KbCategories.AnyAsync(c => c.Id == parentId, ct))
        {
            return Result<int>.FailureCode(HostpanelErrorCodes.KBCATEGORY_NOT_FOUND);
        }

        var category = KbCategory.Create(
            command.Name, command.Description, command.IsHidden, command.ParentCategoryId);

        _context.KbCategories.Add(category);
        await _context.SaveChangesAsync(ct);

        return Result<int>.Success(category.Id);
    }
}
