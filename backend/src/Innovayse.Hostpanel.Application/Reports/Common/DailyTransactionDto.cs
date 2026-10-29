namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>One day row in the Monthly Transactions report.</summary>
/// <param name="Date">The date.</param>
/// <param name="AmountIn">The amount in.</param>
/// <param name="Fees">The fees.</param>
/// <param name="AmountOut">The amount out.</param>
/// <param name="Balance">The balance.</param>
public record DailyTransactionDto(
    string Date,
    decimal AmountIn,
    decimal Fees,
    decimal AmountOut,
    decimal Balance);
