namespace Innovayse.Hostpanel.Application.Reports.Queries.ExportInvoicesReport;

/// <summary>
/// Query for exporting the Invoices report as CSV. Carries the same filters as
/// <see cref="Innovayse.Hostpanel.Application.Reports.Queries.InvoicesReport.InvoicesReportQuery"/>
/// without paging: the export always writes every matching row up to the handler's row ceiling.
/// </summary>
/// <param name="Status">Invoice status to keep, or null for every status.</param>
/// <param name="CreatedFrom">Earliest creation date to include.</param>
/// <param name="CreatedTo">Latest creation date to include.</param>
/// <param name="DueFrom">Earliest due date to include.</param>
/// <param name="DueTo">Latest due date to include.</param>
/// <param name="PaidFrom">Earliest paid date to include.</param>
/// <param name="PaidTo">Latest paid date to include.</param>
public sealed record ExportInvoicesReportQuery(
    string? Status = null,
    DateOnly? CreatedFrom = null,
    DateOnly? CreatedTo = null,
    DateOnly? DueFrom = null,
    DateOnly? DueTo = null,
    DateOnly? PaidFrom = null,
    DateOnly? PaidTo = null);
