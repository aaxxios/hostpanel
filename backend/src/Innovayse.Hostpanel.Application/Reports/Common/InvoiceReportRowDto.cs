using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Domain.Billing.Enums;

namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>One row of the Invoices report.</summary>
/// <param name="Id">The identifier.</param>
/// <param name="ClientId">The client identifier.</param>
/// <param name="ClientName">The client name.</param>
/// <param name="Dates">The created, due, paid and refunded dates.</param>
/// <param name="Currency">The ISO 4217 code the invoice bills in; every amount on the row is in it.</param>
/// <param name="Amounts">The sub total, credit, tax, tax rate and total.</param>
/// <param name="Status">The status.</param>
/// <param name="PaymentMethod">The payment method.</param>
/// <param name="Notes">The notes.</param>
public record InvoiceReportRowDto(
    int Id,
    int ClientId,
    string ClientName,
    InvoiceReportDatesDto Dates,
    string Currency,
    InvoiceAmountsDto Amounts,
    InvoiceStatus Status,
    string? PaymentMethod,
    string? Notes);
