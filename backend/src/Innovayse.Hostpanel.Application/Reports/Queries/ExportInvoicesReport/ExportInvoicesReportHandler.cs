using System.Text;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Reports.Common;
using Innovayse.Hostpanel.Application.Reports.Extensions;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Reports.Queries.ExportInvoicesReport;

/// <summary>Handles <see cref="ExportInvoicesReportQuery"/>.</summary>
public sealed class ExportInvoicesReportHandler
{
    /// <summary>Largest number of rows a single export writes, matching the pre-conversion controller's ceiling.</summary>
    private const int MaxExportRows = 10000;

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

        var invoices = _context.Invoices.AsNoTracking().ApplyReportFilters(
            query.Status, query.CreatedFrom, query.CreatedTo,
            query.DueFrom, query.DueTo, query.PaidFrom, query.PaidTo);

        var rows = await invoices
            .OrderByDescending(i => i.CreatedAt)
            .Take(MaxExportRows)
            .Join(_context.Clients, i => i.ClientId, c => c.Id,
                (i, c) => new { Invoice = i, ClientName = c.FirstName + " " + c.LastName })
            .Select(x => new InvoiceReportRowDto(
                x.Invoice.Id,
                x.Invoice.ClientId,
                x.ClientName,
                null,
                x.Invoice.CreatedAt.ToString("yyyy-MM-dd"),
                x.Invoice.DueDate.ToString("yyyy-MM-dd"),
                x.Invoice.PaidAt != null ? x.Invoice.PaidAt.Value.ToString("yyyy-MM-dd") : null,
                null,
                x.Invoice.Status == InvoiceStatus.Cancelled ? x.Invoice.CreatedAt.ToString("yyyy-MM-dd") : null,
                x.Invoice.SubTotal,
                x.Invoice.Credit,
                x.Invoice.Tax,
                x.Invoice.TaxRate,
                x.Invoice.Total,
                x.Invoice.Status.ToString(),
                x.Invoice.PaymentMethod,
                x.Invoice.Notes))
            .ToListAsync(ct);

        var csv = BuildCsv(rows);
        var dto = new FileDownloadDto(Encoding.UTF8.GetBytes(csv), "text/csv", "invoices-report.csv");
        return Result<FileDownloadDto>.Success(dto);
    }

    /// <summary>Renders the report rows as CSV text, in the pre-conversion controller's exact column order.</summary>
    /// <param name="rows">The rows to render.</param>
    /// <returns>The CSV text, header row first.</returns>
    private static string BuildCsv(IReadOnlyList<InvoiceReportRowDto> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine("ID,Client ID,Client Name,Status,Created Date,Due Date,Date Paid,SubTotal,Credit,Tax,Tax Rate,Total,Payment Method,Notes");
        foreach (var r in rows)
        {
            sb.AppendLine($"{r.Id},{r.ClientId},\"{r.ClientName}\",{r.Status},{r.CreatedDate},{r.DueDate},{r.DatePaid},{r.SubTotal},{r.Credit},{r.Tax},{r.TaxRate},{r.Total},\"{r.PaymentMethod}\",\"{r.Notes}\"");
        }

        return sb.ToString();
    }
}
