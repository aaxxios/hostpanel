namespace Innovayse.Hostpanel.Application.Reports.Queries.TopClientsByIncome;

/// <summary>One row of the Top 10 Clients by Income (transaction-based) report.</summary>
/// <param name="ClientId">The client identifier.</param>
/// <param name="ClientName">The client name.</param>
/// <param name="TotalAmountIn">The total amount in.</param>
/// <param name="TotalFees">The total fees.</param>
/// <param name="TotalAmountOut">The total amount out.</param>
/// <param name="Balance">The balance.</param>
public record TopClientByIncomeDto(
    int ClientId,
    string ClientName,
    decimal TotalAmountIn,
    decimal TotalFees,
    decimal TotalAmountOut,
    decimal Balance);
