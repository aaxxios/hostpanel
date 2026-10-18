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
    /// <returns>The matching page of transactions and the unfiltered total. Never refuses.</returns>
    public async Task<Result<TransactionReportResultDto>> HandleAsync(TransactionsReportQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var transactions = _context.Transactions.AsNoTracking()
            .ApplyReportFilters(query.DateFrom, query.DateTo, query.PaymentMethod);

        var totalCount = await transactions.CountAsync(ct);

        var rows = await transactions
            .OrderByDescending(t => t.Date)
            .Paginate(query.Page, query.PageSize)
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

        return Result<TransactionReportResultDto>.Success(new TransactionReportResultDto(rows, totalCount));
    }
}
