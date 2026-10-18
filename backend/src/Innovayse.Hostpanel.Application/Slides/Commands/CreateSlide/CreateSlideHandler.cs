using System.Text.Json;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Slides;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Slides.Commands.CreateSlide;

/// <summary>
/// Creates a new <see cref="Slide"/> aggregate, applies every translation the command carries, and
/// persists the result.
/// </summary>
public sealed class CreateSlideHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public CreateSlideHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The slide and its translations.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The newly created slide id. This use case refuses nothing.</returns>
    public async Task<Result<int>> HandleAsync(CreateSlideCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var slide = Slide.Create(
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
            cmd.VisibleUntil);

        foreach (var t in cmd.Translations)
        {
            var featuresJson = t.Features is not null
                ? JsonSerializer.Serialize(t.Features)
                : null;

            slide.SetTranslation(t.Locale, t.Title, t.Tagline, t.Description, featuresJson, t.CtaText, t.CtaUrl);
        }

        _context.Slides.Add(slide);
        await _context.SaveChangesAsync(ct);

        return Result<int>.Success(slide.Id);
    }
}
