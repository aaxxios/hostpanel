namespace Innovayse.Hostpanel.Application.Domains.Common;

/// <summary>Top-level response containing TLD pricing data and currency information.</summary>
/// <param name="Currency">Currency information for the pricing values.</param>
/// <param name="Pricing">One entry per offered TLD extension.</param>
public record TldPricingDto(
    TldCurrencyDto Currency,
    IReadOnlyList<TldPriceEntryDto> Pricing);
