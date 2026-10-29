namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>One product row in the grouped Income by Product report.</summary>
/// <param name="ProductId">Identifier of the product.</param>
/// <param name="ProductName">Display name of the product.</param>
/// <param name="UnitsSold">Quantity on the invoice lines paid for it in the month.</param>
/// <param name="TotalIncome">What its paid lines cost the payers: each line's share of its invoice total (tax in, credit out).</param>
public record IncomeByProductRowDto(
    int ProductId,
    string ProductName,
    int UnitsSold,
    decimal TotalIncome);
