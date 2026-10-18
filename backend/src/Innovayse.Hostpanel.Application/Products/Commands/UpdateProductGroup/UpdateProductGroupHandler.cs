using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Products.Commands.UpdateProductGroup;

/// <summary>Updates an existing product group's name and description.</summary>
public sealed class UpdateProductGroupHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public UpdateProductGroupHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The group's new name and description.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>PRODUCT_GROUP_NOT_FOUND</c> when the id names no group.</returns>
    public async Task<Result> HandleAsync(UpdateProductGroupCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var group = await _context.ProductGroups.FirstOrDefaultAsync(g => g.Id == cmd.Id, ct);
        if (group is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.PRODUCT_GROUP_NOT_FOUND);
        }

        group.Update(cmd.Name, cmd.Description);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
