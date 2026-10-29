using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Products.Extensions;
using Innovayse.Hostpanel.Application.Slides.Extensions;
using Innovayse.Hostpanel.Domain.Slides;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Slides.Commands.UpdateSlide;

/// <summary>
/// Loads an existing slide, applies its new field values, replaces its translations, and persists
/// the result.
/// </summary>
public sealed class UpdateSlideHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Wolverine bus the owning modules' existence queries are dispatched through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="bus">Wolverine bus the owning modules' existence queries are dispatched through.</param>
    public UpdateSlideHandler(IHostpanelDbContext context, IMessageBus bus)
    {
        _context = context;
        _bus = bus;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The slide's new fields and translations.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, <c>SLIDE_NOT_FOUND</c> when the id names no slide, or <c>PRODUCT_NOT_FOUND</c> when the product id names no product.
    /// </returns>
    public async Task<Result> HandleAsync(UpdateSlideCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var slide = await _context.Slides.FirstOrDefaultAsync(s => s.Id == cmd.Id, ct);
        if (slide is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.SLIDE_NOT_FOUND);
        }

        if (cmd.ProductId is { } productId && !await _bus.ProductExistsAsync(productId, ct))
        {
            return Result.FailureCode(HostpanelErrorCodes.PRODUCT_NOT_FOUND);
        }

        slide.Reconfigure(new SlideSettings(
            cmd.IconName,
            cmd.BrandColor,
            cmd.ImageUrl,
            cmd.DemoUrl,
            cmd.LearnMoreUrl,
            cmd.ProductId,
            cmd.SortOrder,
            cmd.IsActive,
            cmd.TargetAudience,
            cmd.VisibleFrom,
            cmd.VisibleUntil));

        slide.ReplaceTranslations(cmd.Translations.ToContent());

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
