namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>One row of the Invoices report — mirrors the legacy billing-system fields.</summary>
/// <param name="Id">The identifier.</param>
/// <param name="ClientId">The client identifier.</param>
/// <param name="ClientName">The client name.</param>
/// <param name="InvoiceNumber">The invoice number.</param>
/// <param name="CreatedDate">The created date.</param>
/// <param name="DueDate">The due date.</param>
/// <param name="DatePaid">The date paid.</param>
/// <param name="DateRefunded">The date refunded.</param>
/// <param name="DateCancelled">The date cancelled.</param>
/// <param name="SubTotal">The sub total.</param>
/// <param name="Credit">The credit.</param>
/// <param name="Tax">The tax.</param>
/// <param name="TaxRate">The tax rate.</param>
/// <param name="Total">The total.</param>
/// <param name="Status">The status.</param>
/// <param name="PaymentMethod">The payment method.</param>
/// <param name="Notes">The notes.</param>
public record InvoiceReportRowDto(
    int Id,
    int ClientId,
    string ClientName,
    string? InvoiceNumber,
    string CreatedDate,
    string DueDate,
    string? DatePaid,
    string? DateRefunded,
    string? DateCancelled,
    decimal SubTotal,
    decimal Credit,
    decimal Tax,
    decimal TaxRate,
    decimal Total,
    string Status,
    string? PaymentMethod,
    string? Notes);
