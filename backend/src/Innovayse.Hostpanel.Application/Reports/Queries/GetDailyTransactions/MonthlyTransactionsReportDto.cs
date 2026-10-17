namespace Innovayse.Hostpanel.Application.Reports.Queries.GetDailyTransactions;

/// <summary>Full Monthly Transactions report result.</summary>
/// <param name="Month">The month.</param>
/// <param name="Year">The year.</param>
/// <param name="Rows">The rows entries.</param>
/// <param name="TotalAmountIn">The total amount in.</param>
/// <param name="TotalFees">The total fees.</param>
/// <param name="TotalAmountOut">The total amount out.</param>
public record MonthlyTransactionsReportDto(
    int Month,
    int Year,
    IReadOnlyList<DailyTransactionDto> Rows,
    decimal TotalAmountIn,
    decimal TotalFees,
    decimal TotalAmountOut);
