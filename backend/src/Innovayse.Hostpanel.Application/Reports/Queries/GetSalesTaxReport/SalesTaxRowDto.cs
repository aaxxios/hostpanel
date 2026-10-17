namespace Innovayse.Hostpanel.Application.Reports.Queries.GetSalesTaxReport;

/// <summary>One invoice row in the Sales Tax Liability report.</summary>
/// <param name="Id">The identifier.</param>
/// <param name="ClientName">The client name.</param>
/// <param name="InvoiceDate">The invoice date.</param>
/// <param name="DatePaid">The date paid.</param>
/// <param name="SubTotal">The sub total.</param>
/// <param name="Tax">The tax.</param>
/// <param name="Credit">The credit.</param>
/// <param name="Total">The total.</param>
public record SalesTaxRowDto(
    int Id,
    string ClientName,
    string InvoiceDate,
    string? DatePaid,
    decimal SubTotal,
    decimal Tax,
    decimal Credit,
    decimal Total);
