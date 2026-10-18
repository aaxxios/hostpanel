using System.Text.Json;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Slides.Commands.UpdateSlide;

/// <summary>
/// Loads an existing slide, applies its new field values, replaces its translations, and persists
/// the result.
/// </summary>
public sealed class UpdateSlideHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public UpdateSlideHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The slide's new fields and translations.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>SLIDE_NOT_FOUND</c> when the id names no slide.</returns>
    public async Task<Result> HandleAsync(UpdateSlideCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var slide = await _context.Slides.FirstOrDefaultAsync(s => s.Id == cmd.Id, ct);
        if (slide is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.SLIDE_NOT_FOUND);
        }

        slide.Update(
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

        // Remove stale translations that are no longer in the command.
        var incomingLocales = cmd.Translations.Select(t => t.Locale).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var staleLocales = slide.Translations
            .Select(t => t.Locale)
            .Where(l => !incomingLocales.Contains(l))
            .ToList();

        foreach (var locale in staleLocales)
        {
            slide.RemoveTranslation(locale);
        }

        // Apply every incoming translation.
        foreach (var t in cmd.Translations)
        {
            var featuresJson = t.Features is not null
                ? JsonSerializer.Serialize(t.Features)
                : null;

            slide.SetTranslation(t.Locale, t.Title, t.Tagline, t.Description, featuresJson, t.CtaText, t.CtaUrl);
        }

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
