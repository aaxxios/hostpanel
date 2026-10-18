namespace Innovayse.Hostpanel.Application.Reports.Queries.GetSalesTaxReport;

/// <summary>Full Sales Tax Liability report result.</summary>
/// <param name="TotalInvoices">The total invoices.</param>
/// <param name="TotalInvoiced">The total invoiced.</param>
/// <param name="TaxLevel1Liability">The tax level1 liability.</param>
/// <param name="TaxLevel2Liability">The tax level2 liability.</param>
/// <param name="Rows">The rows entries.</param>
public record SalesTaxReportDto(
    int TotalInvoices,
    decimal TotalInvoiced,
    decimal TaxLevel1Liability,
    decimal TaxLevel2Liability,
    IReadOnlyList<SalesTaxRowDto> Rows);
