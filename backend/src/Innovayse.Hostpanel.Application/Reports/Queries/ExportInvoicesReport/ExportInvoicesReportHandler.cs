using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Csv;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Reports.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Reports.Queries.ExportInvoicesReport;

/// <summary>Handles <see cref="ExportInvoicesReportQuery"/>.</summary>
public sealed class ExportInvoicesReportHandler
{
    /// <summary>Largest number of rows a single export writes.</summary>
    private const int MaxExportRows = 10000;

    /// <summary>The export's column titles, in column order.</summary>
    private static readonly string[] Header =
    [
        "ID", "Client ID", "Client Name", "Status", "Created Date", "Due Date", "Date Paid", "Currency",
        "SubTotal", "Credit", "Tax", "Tax Rate", "Total", "Payment Method", "Notes",
    ];

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public ExportInvoicesReportHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The filters to apply; every matching row up to the row ceiling is written.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The CSV file, header row first, ready for <c>BaseApiController.FileResult</c>. Never refuses.</returns>
    public async Task<Result<FileDownloadDto>> HandleAsync(ExportInvoicesReportQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var rows = await _context.Invoices.AsNoTracking()
            .ApplyReportFilters(
                query.Status, query.CreatedFrom, query.CreatedTo,
                query.DueFrom, query.DueTo, query.PaidFrom, query.PaidTo)
            .OrderByDescending(i => i.CreatedAt)
            .ThenByDescending(i => i.Id)
            .Take(MaxExportRows)
            .SelectReportRows(_context.Clients.AsNoTracking())
            .ToListAsync(ct);

        var csv = new CsvTable(Header);
        foreach (var r in rows)
        {
            csv.AddRow([
                r.Id, r.ClientId, r.ClientName, r.Status, r.Dates.Created, r.Dates.Due, r.Dates.Paid, r.Currency,
                r.Amounts.SubTotal, r.Amounts.Credit, r.Amounts.Tax, r.Amounts.TaxRate, r.Amounts.Total, r.PaymentMethod, r.Notes,
            ]);
        }

        return Result<FileDownloadDto>.Success(csv.ToFile("invoices-report.csv"));
    }
}
