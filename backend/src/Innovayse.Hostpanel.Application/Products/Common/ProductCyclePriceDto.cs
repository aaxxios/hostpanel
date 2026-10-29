using Innovayse.Hostpanel.Domain.Products.Enums;

namespace Innovayse.Hostpanel.Application.Products.Common;

/// <summary>A product's stored price for one currency and billing cycle, as the product itself resolves it.</summary>
/// <param name="ProductId">The priced product.</param>
/// <param name="CurrencyCode">The currency code exactly as the lookup named it.</param>
/// <param name="Cycle">The billing cycle.</param>
/// <param name="Amount">The stored amount, or null when the product is not sold for that cycle in that currency.</param>
public sealed record ProductCyclePriceDto(int ProductId, string CurrencyCode, BillingCycle Cycle, decimal? Amount);
