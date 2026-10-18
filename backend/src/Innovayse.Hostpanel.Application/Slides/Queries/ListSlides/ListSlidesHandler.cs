using System.Text.Json;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Slides.Common;
using Innovayse.Hostpanel.Domain.Slides;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Slides.Queries.ListSlides;

/// <summary>
/// Loads every slide, resolves the linked products' names, and maps them to
/// <see cref="SlideAdminDto"/>.
/// </summary>
public sealed class ListSlidesHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    public ListSlidesHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Handles the query.
    /// </summary>
    /// <param name="query">The query; it carries no input.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Every slide, ordered by sort order ascending. Never refuses.</returns>
    public async Task<Result<IReadOnlyList<SlideAdminDto>>> HandleAsync(ListSlidesQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var slides = await _context.Slides.AsNoTracking().OrderBy(s => s.SortOrder).ToListAsync(ct);

        // Collect every distinct linked product id that needs name resolution.
        var productIds = slides
            .Where(s => s.ProductId.HasValue)
            .Select(s => s.ProductId!.Value)
            .Distinct()
            .ToList();

        var productNameMap = new Dictionary<int, string>();
        if (productIds.Count > 0)
        {
            var products = await _context.Products
                .AsNoTracking()
                .Where(p => productIds.Contains(p.Id))
                .Select(p => new { p.Id, p.Name })
                .ToListAsync(ct);
            foreach (var product in products)
            {
                productNameMap[product.Id] = product.Name;
            }
        }

        var dtos = slides.Select(s => MapToDto(s, productNameMap)).ToList();
        return Result<IReadOnlyList<SlideAdminDto>>.Success(dtos);
    }

    /// <summary>
    /// Maps a <see cref="Slide"/> domain aggregate to a <see cref="SlideAdminDto"/>.
    /// </summary>
    /// <param name="slide">The slide aggregate to map.</param>
    /// <param name="productNameMap">Dictionary of product id to product name for name resolution.</param>
    /// <returns>A fully populated <see cref="SlideAdminDto"/>.</returns>
    private static SlideAdminDto MapToDto(Slide slide, Dictionary<int, string> productNameMap)
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

        var productName = slide.ProductId.HasValue && productNameMap.TryGetValue(slide.ProductId.Value, out var name)
            ? name
            : null;

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
