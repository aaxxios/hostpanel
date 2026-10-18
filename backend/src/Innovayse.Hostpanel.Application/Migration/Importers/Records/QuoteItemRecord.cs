namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One line item on a <see cref="QuoteRecord"/>.</summary>
/// <param name="Description">The description.</param>
/// <param name="UnitPrice">The unit price.</param>
/// <param name="Quantity">The quantity.</param>
/// <param name="DiscountPercent">The discount percent.</param>
internal sealed record QuoteItemRecord(string Description, decimal UnitPrice, int Quantity, decimal DiscountPercent);
