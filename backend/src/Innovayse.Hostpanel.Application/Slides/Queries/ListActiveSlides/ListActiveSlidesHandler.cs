using System.Globalization;
using System.Text.Json;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Common.Settings;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Domain.Slides;
using Innovayse.Hostpanel.Domain.Slides.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
namespace Innovayse.Hostpanel.Application.Slides.Queries.ListActiveSlides;

/// <summary>
/// Loads active slides, filters them by audience, resolves the best-match translation for the
/// requested locale, joins the linked product's pricing, and maps them to
/// <see cref="SlidePublicDto"/>.
/// </summary>
public sealed class ListActiveSlidesHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>The panel fallback locale, for content with no translation in the requested one.</summary>
    private readonly IOptions<LocaleOptions> _localeOptions;

    /// <summary>The currency the caller is billed in; a linked product is priced in it.</summary>
    private readonly IPayerCurrencyResolver _payerCurrency;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="localeOptions">The panel fallback locale, for content with no translation in the requested one.</param>
    /// <param name="payerCurrency">The currency the caller is billed in; a linked product is priced in it.</param>
    public ListActiveSlidesHandler(
        IHostpanelDbContext context, IOptions<LocaleOptions> localeOptions, IPayerCurrencyResolver payerCurrency)
    {
        _context = context;
        _localeOptions = localeOptions;
        _payerCurrency = payerCurrency;
    }

    /// <summary>
    ///     Handles the query.
    /// </summary>
    /// <param name="query">The optional audience filter; the locale is the request culture set from <c>X-Language</c>.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Active slides matching the audience filter (or all if null), ordered by sort order
    /// ascending, mapped with translation and pricing resolved. Never refuses.
    /// </returns>
    public async Task<Result<IReadOnlyList<SlidePublicDto>>> HandleAsync(
        ListActiveSlidesQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var now = DateTimeOffset.UtcNow;
        var slidesQuery = _context.Slides
            .AsNoTracking()
            .Where(s => s.IsActive
                && (s.VisibleFrom == null || s.VisibleFrom <= now)
                && (s.VisibleUntil == null || s.VisibleUntil >= now));

        if (query.Audience.HasValue)
        {
            slidesQuery = slidesQuery.Where(
                s => s.TargetAudience == SlideAudience.All || s.TargetAudience == query.Audience.Value);
        }

        var slides = await slidesQuery.OrderBy(s => s.SortOrder).ToListAsync(ct);

        // Collect every distinct linked product id for batch loading.
        var productIds = slides
            .Where(s => s.ProductId.HasValue)
            .Select(s => s.ProductId!.Value)
            .Distinct()
            .ToList();

        // A slide shows the linked product's price in the visitor's own currency; a cycle the
        // product does not sell in that currency stays null and the slide renders no figure.
        var productMap = new Dictionary<int, (decimal? Monthly, decimal? Annual)>();
        if (productIds.Count > 0)
        {
            var currency = (await _payerCurrency.ForCallerAsync(ct)).Code;
            var products = await _context.Products
                .AsNoTracking()
                .Include(p => p.Prices)
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync(ct);
            foreach (var product in products)
            {
                productMap[product.Id] = (
                    product.PriceFor(currency, BillingCycle.Monthly),
                    product.PriceFor(currency, BillingCycle.Annual));
            }
        }

        var defaultLocale = _localeOptions.Value.DefaultLocale;

        var dtos = slides.Select(s => MapToDto(s, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName, defaultLocale, productMap)).ToList();
        return Result<IReadOnlyList<SlidePublicDto>>.Success(dtos);
    }

    /// <summary>
    /// Maps a <see cref="Slide"/> to a <see cref="SlidePublicDto"/> using locale resolution and product pricing.
    /// </summary>
    /// <param name="slide">The slide aggregate to map.</param>
    /// <param name="requestedLocale">The locale requested by the client.</param>
    /// <param name="defaultLocale">The application default locale as fallback.</param>
    /// <param name="productMap">Map of product id to monthly and annual prices in the caller's currency.</param>
    /// <returns>A <see cref="SlidePublicDto"/> with the best available translation and pricing.</returns>
    private static SlidePublicDto MapToDto(
        Slide slide,
        string requestedLocale,
        string defaultLocale,
        Dictionary<int, (decimal? Monthly, decimal? Annual)> productMap)
    {
        // Locale resolution: requested -> default -> first available.
        var translation =
            slide.Translations.FirstOrDefault(
                t => string.Equals(t.Locale, requestedLocale, StringComparison.OrdinalIgnoreCase))
            ?? slide.Translations.FirstOrDefault(
                t => string.Equals(t.Locale, defaultLocale, StringComparison.OrdinalIgnoreCase))
            ?? slide.Translations.FirstOrDefault();

        decimal? monthlyPrice = null;
        decimal? annualPrice = null;

        if (slide.ProductId.HasValue && productMap.TryGetValue(slide.ProductId.Value, out var pricing))
        {
            monthlyPrice = pricing.Monthly;
            annualPrice = pricing.Annual;
        }

        return new SlidePublicDto(
            slide.Id,
            slide.IconName,
            slide.BrandColor,
            slide.ImageUrl,
            slide.DemoUrl,
            slide.LearnMoreUrl,
            slide.ProductId,
            slide.SortOrder,
            slide.TargetAudience,
            translation?.Title ?? string.Empty,
            translation?.Tagline,
            translation?.Description,
            DeserializeFeatures(translation?.Features),
            translation?.CtaText,
            translation?.CtaUrl,
            monthlyPrice,
            annualPrice);
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
