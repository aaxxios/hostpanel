using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Reports.Common;
using Innovayse.Hostpanel.Application.Reports.Extensions;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Shared.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Reports.Queries.InvoicesReport;

/// <summary>Handles <see cref="InvoicesReportQuery"/>.</summary>
public sealed class InvoicesReportHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public InvoicesReportHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The filters and page to return.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The matching page of invoices and the unfiltered total. Never refuses.</returns>
    public async Task<Result<InvoiceReportResultDto>> HandleAsync(InvoicesReportQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var invoices = _context.Invoices.AsNoTracking().ApplyReportFilters(
            query.Status, query.CreatedFrom, query.CreatedTo,
            query.DueFrom, query.DueTo, query.PaidFrom, query.PaidTo);

        var totalCount = await invoices.CountAsync(ct);

        var rows = await invoices
            .OrderByDescending(i => i.CreatedAt)
            .Paginate(query.Page, query.PageSize)
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

        return Result<InvoiceReportResultDto>.Success(new InvoiceReportResultDto(rows, totalCount));
    }
}
