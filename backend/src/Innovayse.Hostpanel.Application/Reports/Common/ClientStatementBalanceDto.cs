namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>The opening and closing balance of a client statement in one currency.</summary>
/// <param name="Currency">The ISO 4217 code the balances are in.</param>
/// <param name="PreviousBalance">The balance before the first line of the period; positive means the client owes.</param>
/// <param name="EndingBalance">The balance after the last line of the period; positive means the client owes.</param>
public sealed record ClientStatementBalanceDto(string Currency, decimal PreviousBalance, decimal EndingBalance);
