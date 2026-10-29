namespace Innovayse.Hostpanel.Application.Clients.Common;

/// <summary>The count and total of a client's invoices in each status.</summary>
/// <param name="Draft">Draft invoice stats.</param>
/// <param name="Unpaid">Unpaid invoice stats.</param>
/// <param name="Paid">Paid invoice stats.</param>
/// <param name="Overdue">Overdue invoice stats.</param>
/// <param name="Cancelled">Cancelled invoice stats.</param>
/// <param name="Refunded">Refunded invoice stats.</param>
public sealed record ClientInvoiceStatusesDto(
    InvoiceStatusCountDto Draft,
    InvoiceStatusCountDto Unpaid,
    InvoiceStatusCountDto Paid,
    InvoiceStatusCountDto Overdue,
    InvoiceStatusCountDto Cancelled,
    InvoiceStatusCountDto Refunded);
