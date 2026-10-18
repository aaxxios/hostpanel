namespace Innovayse.Hostpanel.Application.Reports.Queries.AgingInvoices;

/// <summary>One row of the Aging Invoices report.</summary>
/// <param name="InvoiceId">The invoice identifier.</param>
/// <param name="Client">The client.</param>
/// <param name="Amount">The amount.</param>
/// <param name="DueDate">The due date.</param>
/// <param name="DaysOutstanding">The days outstanding.</param>
/// <param name="Bucket">The bucket.</param>
public record AgingInvoiceDto(
    int InvoiceId,
    string Client,
    decimal Amount,
    string DueDate,
    int DaysOutstanding,
    string Bucket);
