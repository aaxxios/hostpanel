using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Csv;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Reports.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Reports.Queries.ExportTransactionsReport;

/// <summary>Handles <see cref="ExportTransactionsReportQuery"/>.</summary>
public sealed class ExportTransactionsReportHandler
{
    /// <summary>Largest number of rows a single export writes.</summary>
    private const int MaxExportRows = 10000;

    /// <summary>The export's column titles, in column order.</summary>
    private static readonly string[] Header =
    [
        "ID", "Client ID", "Client Name", "Currency", "Payment Method", "Date", "Description",
        "Invoice ID", "Transaction ID", "Amount In", "Fees", "Amount Out",
    ];

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public ExportTransactionsReportHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The filters to apply; every matching row up to the row ceiling is written.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The CSV file, header row first, ready for <c>BaseApiController.FileResult</c>. Never refuses.</returns>
    public async Task<Result<FileDownloadDto>> HandleAsync(
        ExportTransactionsReportQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var rows = await _context.Transactions.AsNoTracking()
            .ApplyReportFilters(query.DateFrom, query.DateTo, query.PaymentMethod)
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.Id)
            .Take(MaxExportRows)
            .SelectReportRows(_context.Clients.AsNoTracking())
            .ToListAsync(ct);

        var csv = new CsvTable(Header);
        foreach (var r in rows)
        {
            csv.AddRow([
                r.Id, r.ClientId, r.ClientName, r.Currency, r.PaymentMethod, r.Date, r.Description,
                r.InvoiceId, r.TransactionId, r.AmountIn, r.Fees, r.AmountOut,
            ]);
        }

        return Result<FileDownloadDto>.Success(csv.ToFile("transactions-report.csv"));
    }
}
