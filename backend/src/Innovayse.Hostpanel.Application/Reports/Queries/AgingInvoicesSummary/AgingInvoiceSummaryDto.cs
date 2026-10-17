namespace Innovayse.Hostpanel.Application.Reports.Queries.AgingInvoicesSummary;

/// <summary>Full Aging Invoices summary with periods and totals.</summary>
/// <param name="Periods">The periods entries.</param>
/// <param name="Totals">The totals.</param>
/// <param name="Currencies">The currencies entries.</param>
public record AgingInvoiceSummaryDto(
    IReadOnlyList<AgingPeriodDto> Periods,
    Dictionary<string, decimal> Totals,
    IReadOnlyList<string> Currencies);
