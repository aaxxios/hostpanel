using Innovayse.Hostpanel.Application.Billing.Common;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Shared.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Billing.Queries.ListTransactions;

/// <summary>Returns a paginated list of transactions with financial summary totals.</summary>
public sealed class ListTransactionsHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public ListTransactionsHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles <see cref="ListTransactionsQuery"/>.</summary>
    /// <param name="query">The query with client ID and pagination params.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with the paginated transactions and financial summary. Never refuses.</returns>
    public async Task<Result<TransactionsResultDto>> HandleAsync(
        ListTransactionsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        IQueryable<Transaction> filtered = _context.Transactions.AsNoTracking();
        if (query.ClientId.HasValue)
        {
            filtered = filtered.Where(t => t.ClientId == query.ClientId.Value);
        }

        var ordered = filtered.OrderByDescending(t => t.Date);
        var totalCount = await ordered.CountAsync(ct);
        var items = await ordered
            .Paginate(query.Page, query.PageSize)
            .ToListAsync(ct);

        decimal totalIn = 0m;
        decimal totalOut = 0m;
        decimal totalFees = 0m;
        if (query.ClientId.HasValue)
        {
            var summary = await _context.Transactions
                .AsNoTracking()
                .Where(t => t.ClientId == query.ClientId.Value)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    TotalIn = g.Sum(t => t.AmountIn),
                    TotalOut = g.Sum(t => t.AmountOut),
                    TotalFees = g.Sum(t => t.Fees),
                })
                .FirstOrDefaultAsync(ct);

            if (summary is not null)
            {
                totalIn = summary.TotalIn;
                totalOut = summary.TotalOut;
                totalFees = summary.TotalFees;
            }
        }

        var clientIds = items.Select(t => t.ClientId).Distinct().ToList();
        var clientMap = await _context.Clients
            .AsNoTracking()
            .Where(c => clientIds.Contains(c.Id))
            .Select(c => new { c.Id, Name = (c.FirstName + " " + c.LastName).Trim() })
            .ToDictionaryAsync(c => c.Id, c => c.Name, ct);

        var dtos = items.Select(t => new TransactionDto(
            t.Id, t.ClientId,
            clientMap.TryGetValue(t.ClientId, out var name) ? name : $"Client #{t.ClientId}",
            t.Date, t.Description, t.TransactionId,
            t.InvoiceId, t.PaymentMethod, t.AmountIn, t.AmountOut, t.Fees, t.AddedToCredit))
            .ToList();

        var paged = new PagedResult<TransactionDto>(dtos, totalCount, query.Page, query.PageSize);
        var balance = totalIn - totalOut - totalFees;

        return Result<TransactionsResultDto>.Success(
            new TransactionsResultDto(paged, totalIn, totalOut, totalFees, balance));
    }
}
