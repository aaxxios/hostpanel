namespace Innovayse.Hostpanel.Application.Billing.Commands.CreateQuote;

/// <summary>A single line item to add when a quote is created.</summary>
/// <param name="Description">Human-readable description.</param>
/// <param name="UnitPrice">Price per unit (≥ 0).</param>
/// <param name="Quantity">Number of units (≥ 1).</param>
/// <param name="DiscountPercent">Discount percentage (0–100). Defaults to 0.</param>
/// <param name="Taxed">Whether this item is taxed. Defaults to false.</param>
public sealed record QuoteItemEntry(
    string Description, decimal UnitPrice, int Quantity, decimal DiscountPercent = 0, bool Taxed = false);
