using System.Text;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Reports.Common;
using Innovayse.Hostpanel.Application.Reports.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Reports.Queries.ExportTransactionsReport;

/// <summary>Handles <see cref="ExportTransactionsReportQuery"/>.</summary>
public sealed class ExportTransactionsReportHandler
{
    /// <summary>Largest number of rows a single export writes, matching the pre-conversion controller's ceiling.</summary>
    private const int MaxExportRows = 10000;

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

        var transactions = _context.Transactions.AsNoTracking()
            .ApplyReportFilters(query.DateFrom, query.DateTo, query.PaymentMethod);

        var rows = await transactions
            .OrderByDescending(t => t.Date)
            .Take(MaxExportRows)
            .Join(_context.Clients, t => t.ClientId, c => c.Id,
                (t, c) => new { Transaction = t, ClientName = c.FirstName + " " + c.LastName })
            .Select(x => new TransactionReportRowDto(
                x.Transaction.Id,
                x.Transaction.ClientId,
                x.ClientName,
                // The report has always shown "USD" here regardless of the transaction's own
                // currency; kept exactly, not "fixed" into a semantic change.
                "USD",
                x.Transaction.PaymentMethod,
                x.Transaction.Date.ToString("yyyy-MM-dd HH:mm:ss"),
                x.Transaction.Description,
                null,
                x.Transaction.TransactionId,
                x.Transaction.AmountIn,
                x.Transaction.Fees,
                x.Transaction.AmountOut))
            .ToListAsync(ct);

        var csv = BuildCsv(rows);
        var dto = new FileDownloadDto(Encoding.UTF8.GetBytes(csv), "text/csv", "transactions-report.csv");
        return Result<FileDownloadDto>.Success(dto);
    }

    /// <summary>Renders the report rows as CSV text, in the pre-conversion controller's exact column order.</summary>
    /// <param name="rows">The rows to render.</param>
    /// <returns>The CSV text, header row first.</returns>
    private static string BuildCsv(IReadOnlyList<TransactionReportRowDto> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine("ID,Client ID,Client Name,Currency,Payment Method,Date,Description,Invoice ID,Transaction ID,Amount In,Fees,Amount Out");
        foreach (var r in rows)
        {
            sb.AppendLine($"{r.Id},{r.ClientId},\"{r.ClientName}\",{r.Currency},\"{r.PaymentMethod}\",{r.Date},\"{r.Description}\",{r.InvoiceId},{r.TransactionId},{r.AmountIn},{r.Fees},{r.AmountOut}");
        }

        return sb.ToString();
    }
}
