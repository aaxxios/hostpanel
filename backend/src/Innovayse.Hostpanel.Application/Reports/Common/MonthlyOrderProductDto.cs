namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>One product row in the Monthly Orders report.</summary>
/// <param name="ProductId">Identifier of the ordered product.</param>
/// <param name="ProductName">Display name of the product.</param>
/// <param name="UnitsSold">Units ordered in the month: one per order line, on orders that are pending or accepted (cancelled and fraud orders sell nothing).</param>
/// <param name="Value">First payment value of those lines, converted to the base currency.</param>
public record MonthlyOrderProductDto(
    int ProductId,
    string ProductName,
    int UnitsSold,
    decimal Value);
