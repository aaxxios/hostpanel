namespace Innovayse.Hostpanel.Application.Reports.Queries.AnnualIncome;

/// <summary>One row of the Annual Income report.</summary>
/// <param name="Month">The month.</param>
/// <param name="Amount">The amount.</param>
public record AnnualIncomeDto(string Month, decimal Amount);
