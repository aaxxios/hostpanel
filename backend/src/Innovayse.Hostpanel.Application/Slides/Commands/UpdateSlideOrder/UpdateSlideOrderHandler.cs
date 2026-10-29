using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Extensions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Slides.Commands.UpdateSlideOrder;

/// <summary>
/// Loads the slides the command names, applies their new sort order values, and persists the
/// reordering in one transaction. Slides whose ids are not present in the command are
/// left unchanged.
/// </summary>
public sealed class UpdateSlideOrderHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public UpdateSlideOrderHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The slide id and new sort order pairs.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>SLIDE_NOT_FOUND</c> when an id names no row; nothing is moved then.</returns>
    public async Task<Result> HandleAsync(UpdateSlideOrderCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        if (!await _context.Slides.ApplySortOrderAsync(cmd.Items, ct))
        {
            return Result.FailureCode(HostpanelErrorCodes.SLIDE_NOT_FOUND);
        }

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
