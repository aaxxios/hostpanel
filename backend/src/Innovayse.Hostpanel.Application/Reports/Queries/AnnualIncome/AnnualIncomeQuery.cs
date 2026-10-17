namespace Innovayse.Hostpanel.Application.Reports.Queries.AnnualIncome;

/// <summary>Query for the Annual Income report.</summary>
/// <param name="Year">The year to report on, or <see langword="null"/> for the current UTC year.</param>
public sealed record AnnualIncomeQuery(int? Year = null);
