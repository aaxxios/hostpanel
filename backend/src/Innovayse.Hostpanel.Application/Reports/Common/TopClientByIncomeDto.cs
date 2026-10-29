namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>
/// One row of the Top Clients by Income report: the client's invoice income, every amount
/// converted to <paramref name="Currency"/>.
/// </summary>
/// <param name="ClientId">The client identifier.</param>
/// <param name="ClientName">The client's first and last name.</param>
/// <param name="TotalAmountIn">Money taken on the client's invoices.</param>
/// <param name="TotalFees">Gateway fees recorded on those payments and refunds.</param>
/// <param name="TotalAmountOut">Money refunded on the client's invoices.</param>
/// <param name="Balance">Net income: money taken less refunds and fees; the report ranks by it.</param>
/// <param name="Currency">ISO 4217 code of the base currency the amounts are converted to.</param>
public sealed record TopClientByIncomeDto(
    int ClientId,
    string ClientName,
    decimal TotalAmountIn,
    decimal TotalFees,
    decimal TotalAmountOut,
    decimal Balance,
    string Currency);
