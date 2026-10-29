namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One line item on a <see cref="QuoteRecord"/>.</summary>
/// <param name="Description">The line text shown on the quote.</param>
/// <param name="UnitPrice">Price of one unit, in the client's currency.</param>
/// <param name="Quantity">How many units the line bills.</param>
/// <param name="DiscountPercent">Discount on the line, 0 to 100.</param>
internal sealed record QuoteItemRecord(string Description, decimal UnitPrice, int Quantity, decimal DiscountPercent);
