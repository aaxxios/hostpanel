namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>One invoice row in the Sales Tax Liability report.</summary>
/// <param name="Id">The invoice identifier.</param>
/// <param name="ClientName">The client's full name.</param>
/// <param name="InvoiceDate">The day the invoice was raised (UTC), <c>yyyy-MM-dd</c>.</param>
/// <param name="DatePaid">The day it was paid (UTC), <c>yyyy-MM-dd</c>.</param>
/// <param name="Currency">The ISO 4217 code the invoice bills in; every amount on the row is in it.</param>
/// <param name="SubTotal">The sub total.</param>
/// <param name="Tax">The tax.</param>
/// <param name="Credit">The credit applied.</param>
/// <param name="Total">The total.</param>
public sealed record SalesTaxRowDto(
    int Id,
    string ClientName,
    string InvoiceDate,
    string DatePaid,
    string Currency,
    decimal SubTotal,
    decimal Tax,
    decimal Credit,
    decimal Total);
