using System.Text.Json;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Slides.Common;
using Innovayse.Hostpanel.Domain.Slides;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Slides.Queries.GetSlide;

/// <summary>
/// Loads a single slide, resolves its linked product's name if it has one, and maps it to
/// <see cref="SlideAdminDto"/>.
/// </summary>
public sealed class GetSlideHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public GetSlideHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the query.
    /// </summary>
    /// <param name="query">The slide id to retrieve.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The admin DTO, or <c>SLIDE_NOT_FOUND</c> when the id names no slide.</returns>
    public async Task<Result<SlideAdminDto>> HandleAsync(GetSlideQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var slide = await _context.Slides.AsNoTracking().FirstOrDefaultAsync(s => s.Id == query.Id, ct);
        if (slide is null)
        {
            return Result<SlideAdminDto>.FailureCode(HostpanelErrorCodes.SLIDE_NOT_FOUND);
        }

        string? productName = null;
        if (slide.ProductId.HasValue)
        {
            productName = await _context.Products
                .AsNoTracking()
                .Where(p => p.Id == slide.ProductId.Value)
                .Select(p => p.Name)
                .FirstOrDefaultAsync(ct);
        }

        return Result<SlideAdminDto>.Success(MapToDto(slide, productName));
    }

    /// <summary>
    /// Maps a <see cref="Slide"/> domain aggregate to a <see cref="SlideAdminDto"/>.
    /// </summary>
    /// <param name="slide">The slide aggregate to map.</param>
    /// <param name="productName">The resolved product name, or null.</param>
    /// <returns>A fully populated <see cref="SlideAdminDto"/>.</returns>
    private static SlideAdminDto MapToDto(Slide slide, string? productName)
    {
        var translations = slide.Translations
            .Select(t => new SlideTranslationDto(
                t.Locale,
                t.Title,
                t.Tagline,
                t.Description,
                DeserializeFeatures(t.Features),
                t.CtaText,
                t.CtaUrl))
            .ToList();

        return new SlideAdminDto(
            slide.Id,
            slide.IconName,
            slide.BrandColor,
            slide.ImageUrl,
            slide.DemoUrl,
            slide.LearnMoreUrl,
            slide.ProductId,
            productName,
            slide.SortOrder,
            slide.IsActive,
            slide.TargetAudience,
            slide.VisibleFrom,
            slide.VisibleUntil,
            slide.CreatedAt,
            translations);
    }

    /// <summary>
    /// Deserializes a JSON features string to a string array, returning null when the input is
    /// null or invalid.
    /// </summary>
    /// <param name="featuresJson">The JSON string representing the features array, or null.</param>
    /// <returns>Deserialized string array, or null if the input is null or cannot be parsed.</returns>
    private static string[]? DeserializeFeatures(string? featuresJson)
    {
        if (featuresJson is null)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<string[]>(featuresJson);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
