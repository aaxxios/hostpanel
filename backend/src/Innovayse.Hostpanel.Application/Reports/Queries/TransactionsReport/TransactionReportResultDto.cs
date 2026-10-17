using Innovayse.Hostpanel.Application.Reports.Common;

namespace Innovayse.Hostpanel.Application.Reports.Queries.TransactionsReport;

/// <summary>Paginated result for the Transactions report.</summary>
/// <param name="Items">The items entries.</param>
/// <param name="TotalCount">The total count.</param>
public record TransactionReportResultDto(
    IReadOnlyList<TransactionReportRowDto> Items,
    int TotalCount);
