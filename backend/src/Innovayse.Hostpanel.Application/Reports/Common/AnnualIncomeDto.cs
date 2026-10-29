namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>One month of the Annual Income report.</summary>
/// <param name="Month">Three-letter month label, January first.</param>
/// <param name="Amount">Income of the month: payments less refunds, converted to the base currency.</param>
/// <param name="Currency">ISO 4217 code of the base currency <paramref name="Amount"/> is in.</param>
public sealed record AnnualIncomeDto(string Month, decimal Amount, string Currency);
