namespace Innovayse.Hostpanel.Application.Reports.Queries.ExportTransactionsReport;

/// <summary>
/// Query for exporting the Transactions report as CSV. Carries the same filters as
/// <see cref="Innovayse.Hostpanel.Application.Reports.Queries.TransactionsReport.TransactionsReportQuery"/>
/// without paging: the export always writes every matching row up to the handler's row ceiling.
/// </summary>
/// <param name="DateFrom">Earliest transaction date to include.</param>
/// <param name="DateTo">Latest transaction date to include.</param>
/// <param name="PaymentMethod">Payment method to keep, or null for every method.</param>
public sealed record ExportTransactionsReportQuery(
    DateOnly? DateFrom = null,
    DateOnly? DateTo = null,
    string? PaymentMethod = null);
