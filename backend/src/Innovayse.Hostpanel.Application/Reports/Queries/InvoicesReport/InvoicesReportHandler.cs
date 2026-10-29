using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Reports.Common;
using Innovayse.Hostpanel.Application.Reports.Extensions;
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
    /// <returns>The matching page of invoices, newest first, and the count of every match. Never refuses.</returns>
    public async Task<Result<PaginatedList<InvoiceReportRowDto>>> HandleAsync(InvoicesReportQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var invoices = _context.Invoices.AsNoTracking().ApplyReportFilters(
            query.Status, query.CreatedFrom, query.CreatedTo,
            query.DueFrom, query.DueTo, query.PaidFrom, query.PaidTo);

        var totalCount = await invoices.CountAsync(ct);

        var rows = await invoices
            .OrderByDescending(i => i.CreatedAt)
            .ThenByDescending(i => i.Id)
            .Paginate(query.Page, query.PageSize)
            .SelectReportRows(_context.Clients.AsNoTracking())
            .ToListAsync(ct);

        return Result<PaginatedList<InvoiceReportRowDto>>.Success(new PaginatedList<InvoiceReportRowDto>(rows, totalCount, query.Page, query.PageSize));
    }
}
