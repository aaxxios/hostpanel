using Innovayse.Hostpanel.Application.Reports.Common;

namespace Innovayse.Hostpanel.Application.Reports.Queries.InvoicesReport;

/// <summary>Paginated result for the Invoices report.</summary>
/// <param name="Items">The items entries.</param>
/// <param name="TotalCount">The total count.</param>
public record InvoiceReportResultDto(
    IReadOnlyList<InvoiceReportRowDto> Items,
    int TotalCount);
