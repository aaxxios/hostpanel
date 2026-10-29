using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Reports.Common;
using Innovayse.Hostpanel.Application.Reports.Extensions;
using Innovayse.Hostpanel.Shared.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Reports.Queries.TransactionsReport;

/// <summary>Handles <see cref="TransactionsReportQuery"/>.</summary>
public sealed class TransactionsReportHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public TransactionsReportHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The filters and page to return.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The matching page of transactions, newest first, and the count of every match. Never refuses.</returns>
    public async Task<Result<PaginatedList<TransactionReportRowDto>>> HandleAsync(TransactionsReportQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var transactions = _context.Transactions.AsNoTracking()
            .ApplyReportFilters(query.DateFrom, query.DateTo, query.PaymentMethod);

        var totalCount = await transactions.CountAsync(ct);

        var rows = await transactions
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.Id)
            .Paginate(query.Page, query.PageSize)
            .SelectReportRows(_context.Clients.AsNoTracking())
            .ToListAsync(ct);

        return Result<PaginatedList<TransactionReportRowDto>>.Success(new PaginatedList<TransactionReportRowDto>(rows, totalCount, query.Page, query.PageSize));
    }
}
