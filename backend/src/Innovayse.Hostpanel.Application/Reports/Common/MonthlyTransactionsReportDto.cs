namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>Full Monthly Transactions report result; every amount is converted to <paramref name="Currency"/>.</summary>
/// <param name="Month">The month.</param>
/// <param name="Year">The year.</param>
/// <param name="Rows">The rows entries.</param>
/// <param name="TotalAmountIn">The total amount in.</param>
/// <param name="TotalFees">The total fees.</param>
/// <param name="TotalAmountOut">The total amount out.</param>
/// <param name="Currency">ISO 4217 code of the base currency the amounts are converted to.</param>
public sealed record MonthlyTransactionsReportDto(
    int Month,
    int Year,
    IReadOnlyList<DailyTransactionDto> Rows,
    decimal TotalAmountIn,
    decimal TotalFees,
    decimal TotalAmountOut,
    string Currency);
