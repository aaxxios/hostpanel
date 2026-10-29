using Innovayse.Hostpanel.Domain.Products.Enums;

namespace Innovayse.Hostpanel.Application.Products.Common;

/// <summary>One product price another module needs: a product, a currency and a billing cycle.</summary>
/// <param name="ProductId">The product to price.</param>
/// <param name="CurrencyCode">ISO 4217 alpha code, any case.</param>
/// <param name="Cycle">The billing cycle.</param>
public sealed record ProductCyclePriceLookup(int ProductId, string CurrencyCode, BillingCycle Cycle);
