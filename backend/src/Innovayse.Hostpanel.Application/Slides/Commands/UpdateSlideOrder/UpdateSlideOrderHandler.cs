using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Slides.Commands.UpdateSlideOrder;

/// <summary>
/// Loads every slide, applies the new sort order values for the ids listed in the command, and
/// persists the reordering in one transaction. Slides whose ids are not present in the command are
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
    /// <returns>Success. Unknown ids in the list are silently skipped.</returns>
    public async Task<Result> HandleAsync(UpdateSlideOrderCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var slides = await _context.Slides.OrderBy(s => s.SortOrder).ToListAsync(ct);
        var orderMap = cmd.Items.ToDictionary(i => i.Id, i => i.SortOrder);

        foreach (var slide in slides)
        {
            if (orderMap.TryGetValue(slide.Id, out var newOrder))
            {
                slide.Update(
                    slide.IconName,
                    slide.BrandColor,
                    slide.ImageUrl,
                    slide.DemoUrl,
                    slide.LearnMoreUrl,
                    slide.ProductId,
                    newOrder,
                    slide.IsActive,
                    slide.TargetAudience,
                    slide.VisibleFrom,
                    slide.VisibleUntil);
            }
        }

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
