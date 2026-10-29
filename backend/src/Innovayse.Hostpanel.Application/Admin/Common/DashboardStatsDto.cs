namespace Innovayse.Hostpanel.Application.Admin.Common;

/// <summary>
/// Aggregated statistics for the admin dashboard.
/// </summary>
/// <param name="TotalRevenue">Income across all time: invoice payments less refunds, in <paramref name="Currency"/>.</param>
/// <param name="MonthlyRevenue">Income in the last 30 days, the same way, in <paramref name="Currency"/>.</param>
/// <param name="ActiveServices">Number of services in Active status.</param>
/// <param name="OverdueInvoices">Number of invoices in Overdue status.</param>
/// <param name="OpenTickets">Number of support tickets in Open status.</param>
/// <param name="TotalClients">Total number of registered client accounts.</param>
/// <param name="Currency">ISO 4217 code of the base currency the revenue figures are converted to.</param>
public sealed record DashboardStatsDto(
    decimal TotalRevenue,
    decimal MonthlyRevenue,
    int ActiveServices,
    int OverdueInvoices,
    int OpenTickets,
    int TotalClients,
    string Currency);
