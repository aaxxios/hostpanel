namespace Innovayse.Hostpanel.Domain.Products;

/// <summary>Every price a product has in one currency, one optional amount per billing cycle.</summary>
/// <param name="CurrencyCode">ISO 4217 alpha code, any case.</param>
/// <param name="Monthly">Monthly amount, or <see langword="null"/> when the product is not sold monthly in this currency.</param>
/// <param name="Annual">Annual amount, or <see langword="null"/> when the product is not sold yearly in this currency.</param>
public sealed record ProductCurrencyPrices(string CurrencyCode, decimal? Monthly, decimal? Annual);
