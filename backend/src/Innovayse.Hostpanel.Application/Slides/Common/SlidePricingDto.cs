namespace Innovayse.Hostpanel.Application.Slides.Common;

/// <summary>The price of the product a slide links to, in the caller's currency.</summary>
/// <param name="MonthlyPrice">Monthly price, or null when there is no product or no such price.</param>
/// <param name="AnnualPrice">Annual price, or null when there is no product or no such price.</param>
public sealed record SlidePricingDto(decimal? MonthlyPrice, decimal? AnnualPrice);
