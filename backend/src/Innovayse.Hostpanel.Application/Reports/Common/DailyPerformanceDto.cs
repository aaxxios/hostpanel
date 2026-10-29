namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>One row of the Daily Performance report.</summary>
/// <param name="Date">The UTC day, as yyyy-MM-dd.</param>
/// <param name="CompletedOrders">Orders placed that day that were accepted (Active); pending, cancelled and fraud orders are not completed.</param>
/// <param name="NewInvoices">Invoices raised that day.</param>
/// <param name="PaidInvoices">Invoices paid that day, by the moment they were paid.</param>
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
