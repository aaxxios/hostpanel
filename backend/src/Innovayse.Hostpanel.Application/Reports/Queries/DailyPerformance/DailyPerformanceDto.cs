namespace Innovayse.Hostpanel.Application.Reports.Queries.DailyPerformance;

/// <summary>One row of the Daily Performance report.</summary>
/// <param name="Date">The date.</param>
/// <param name="CompletedOrders">The completed orders.</param>
/// <param name="NewInvoices">The new invoices.</param>
/// <param name="PaidInvoices">The paid invoices.</param>
/// <param name="OpenedTickets">The opened tickets.</param>
/// <param name="TicketReplies">The ticket replies.</param>
/// <param name="CancellationRequests">The cancellation requests.</param>
public record DailyPerformanceDto(
    string Date,
    int CompletedOrders,
    int NewInvoices,
    int PaidInvoices,
    int OpenedTickets,
    int TicketReplies,
    int CancellationRequests);
