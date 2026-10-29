using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Billing.Common;
/// <summary>
/// Wraps a paginated list of transactions with financial summary totals.
/// </summary>
/// <param name="Transactions">Paginated transaction list.</param>
/// <param name="TotalIn">Sum of all AmountIn across the matching transactions, each converted to the base currency.</param>
/// <param name="TotalOut">Sum of all AmountOut across the matching transactions, each converted to the base currency.</param>
/// <param name="TotalFees">Sum of all Fees across the matching transactions, each converted to the base currency.</param>
/// <param name="Balance">Net balance: TotalIn − TotalOut − TotalFees.</param>
/// <param name="Currency">ISO 4217 code of the base currency the four totals are expressed in.</param>
public sealed record TransactionsResultDto(
    PaginatedList<TransactionDto> Transactions,
    decimal TotalIn,
    decimal TotalOut,
    decimal TotalFees,
    decimal Balance,
    string Currency);
