using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Support;
using Innovayse.Hostpanel.Shared.Models;

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
    /// <returns>Success with the new category's id. This use case answers no refusal code.</returns>
    public async Task<Result<int>> HandleAsync(CreateDownloadCategoryCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var category = DownloadCategory.Create(cmd.Name, cmd.Description, cmd.IsHidden, cmd.ParentCategoryId);

        _context.DownloadCategories.Add(category);
        await _context.SaveChangesAsync(ct);

        return Result.Success(category.Id);
    }
}
