using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Products.Extensions;
using Innovayse.Hostpanel.Application.Slides.Extensions;
using Innovayse.Hostpanel.Domain.Slides;
using Innovayse.Hostpanel.Shared.Models;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Slides.Commands.CreateSlide;

/// <summary>
/// Creates a new <see cref="Slide"/> aggregate, applies every translation the command carries, and
/// persists the result.
/// </summary>
public sealed class CreateSlideHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>The clock that dates the new slide.</summary>
    private readonly IClock _clock;

    /// <summary>Wolverine bus the owning modules' existence queries are dispatched through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="clock">The clock that dates the new slide.</param>
    /// <param name="bus">Wolverine bus the owning modules' existence queries are dispatched through.</param>
    public CreateSlideHandler(IHostpanelDbContext context, IClock clock, IMessageBus bus)
    {
        _context = context;
        _clock = clock;
        _bus = bus;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The slide and its translations.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The newly created slide id, or <c>PRODUCT_NOT_FOUND</c> when the product id names no product.</returns>
    public async Task<Result<int>> HandleAsync(CreateSlideCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        if (cmd.ProductId is { } productId && !await _bus.ProductExistsAsync(productId, ct))
        {
            return Result<int>.FailureCode(HostpanelErrorCodes.PRODUCT_NOT_FOUND);
        }

        var slide = Slide.Create(new SlideSettings(
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
            cmd.VisibleUntil),
            _clock.UtcNow);

        slide.ReplaceTranslations(cmd.Translations.ToContent());

        _context.Slides.Add(slide);
        await _context.SaveChangesAsync(ct);

        return Result<int>.Success(slide.Id);
    }
}
