namespace Innovayse.Hostpanel.Application.Billing.Common;

/// <summary>The income of each month of one calendar year, converted to the base currency.</summary>
/// <param name="Currency">ISO 4217 code of the base currency the amounts are in.</param>
/// <param name="Months">Twelve amounts, January first; zero for a month with no income.</param>
public sealed record YearIncome(string Currency, IReadOnlyList<decimal> Months);
