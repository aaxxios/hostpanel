using Innovayse.Hostpanel.Application.Notifications.Common;

namespace Innovayse.Hostpanel.Application.Clients.Common;

/// <summary>Aggregated summary data for a client profile dashboard.</summary>
/// <param name="Invoices">Invoice count and total per status.</param>
/// <param name="Transactions">Income, outgoing and fee totals from transactions.</param>
/// <param name="CreditBalance">Current credit balance from Client entity.</param>
/// <param name="Counts">Number of services, domains, quotes and tickets.</param>
/// <param name="RecentEmails">Last 5 email log entries sent to this client.</param>
public sealed record ClientSummaryDto(
    ClientInvoiceStatusesDto Invoices,
    ClientTransactionTotalsDto Transactions,
    decimal CreditBalance,
    ClientRecordCountsDto Counts,
    IReadOnlyList<EmailLogDto> RecentEmails);
