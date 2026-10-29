namespace Innovayse.Hostpanel.Application.Clients.Common;

/// <summary>What a client's transactions add up to.</summary>
/// <param name="GrossRevenue">Total income from transactions.</param>
/// <param name="ClientExpenses">Total outgoing from transactions.</param>
/// <param name="TotalFees">Total fees from transactions.</param>
/// <param name="NetIncome">Computed: GrossRevenue - ClientExpenses - TotalFees.</param>
public sealed record ClientTransactionTotalsDto(
    decimal GrossRevenue,
    decimal ClientExpenses,
    decimal TotalFees,
    decimal NetIncome);
