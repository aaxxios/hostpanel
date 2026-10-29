namespace Innovayse.Hostpanel.Application.Migration.Common;

/// <summary>How many billing records the legacy source holds.</summary>
/// <param name="Invoices">Invoices available to pull.</param>
/// <param name="Orders">Orders available to pull.</param>
/// <param name="Transactions">Payment transactions available to pull.</param>
/// <param name="Quotes">Quotes available to pull.</param>
public sealed record MigrationBillingTotals(int Invoices = 0, int Orders = 0, int Transactions = 0, int Quotes = 0);
