namespace Innovayse.Hostpanel.Application.Reports.Queries.AgingInvoicesSummary;

/// <summary>One period row of the Aging Invoices summary report.</summary>
/// <param name="Period">The period.</param>
/// <param name="AmountsByCurrency">The amounts by currency.</param>
public record AgingPeriodDto(
    string Period,
    Dictionary<string, decimal> AmountsByCurrency);
