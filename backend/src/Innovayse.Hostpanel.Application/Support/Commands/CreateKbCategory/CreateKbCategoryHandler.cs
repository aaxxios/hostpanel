using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Support;
using Innovayse.Hostpanel.Shared.Models;

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
    /// <returns>The new category's id. This use case refuses nothing.</returns>
    public async Task<Result<int>> HandleAsync(CreateKbCategoryCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var category = KbCategory.Create(
            command.Name, command.Description, command.IsHidden, command.ParentCategoryId);

        _context.KbCategories.Add(category);
        await _context.SaveChangesAsync(ct);

        return Result<int>.Success(category.Id);
    }
}
