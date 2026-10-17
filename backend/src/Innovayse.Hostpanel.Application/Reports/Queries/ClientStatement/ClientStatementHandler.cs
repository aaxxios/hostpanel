using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Reports.Queries.ClientStatement;

/// <summary>Handles <see cref="ClientStatementQuery"/>.</summary>
public sealed class ClientStatementHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public ClientStatementHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The client and date range to statement.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The statement with its running balance, or <c>CLIENT_NOT_FOUND</c> when the id names no
    /// client.
    /// </returns>
    public async Task<Result<ClientStatementDto>> HandleAsync(ClientStatementQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var fromDate = query.From?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) ?? DateTimeOffset.MinValue;
        var toDate = query.To?.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc) ?? DateTimeOffset.UtcNow;

        var client = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == query.ClientId, ct);
        if (client is null)
        {
            return Result<ClientStatementDto>.FailureCode(HostpanelErrorCodes.CLIENT_NOT_FOUND);
        }

        var clientName = $"{client.FirstName} {client.LastName}";

        // Previous balance: net of every invoice and transaction before the period.
        var prevInvoiceDebit = await _context.Invoices
            .AsNoTracking()
            .Where(i => i.ClientId == query.ClientId && i.CreatedAt < fromDate)
            .SumAsync(i => i.Total, ct);

        var prevTxCredit = await _context.Transactions
            .AsNoTracking()
            .Where(t => t.ClientId == query.ClientId && t.Date < fromDate)
            .SumAsync(t => t.AmountIn - t.AmountOut, ct);

        var previousBalance = prevInvoiceDebit - prevTxCredit;

        var invoices = await _context.Invoices
            .AsNoTracking()
            .Where(i => i.ClientId == query.ClientId && i.CreatedAt >= fromDate && i.CreatedAt <= toDate)
            .OrderBy(i => i.CreatedAt)
            .Select(i => new { i.Id, i.CreatedAt, i.Total })
            .ToListAsync(ct);

        var transactions = await _context.Transactions
            .AsNoTracking()
            .Where(t => t.ClientId == query.ClientId && t.Date >= fromDate && t.Date <= toDate)
            .OrderBy(t => t.Date)
            .Select(t => new { t.Id, t.Date, t.Description, t.AmountIn, t.AmountOut })
            .ToListAsync(ct);

        // Merged and sorted client-side: the two sources have unrelated shapes and the running
        // balance below needs one interleaved timeline, which SQL cannot hand back directly.
        var lines = new List<(DateTimeOffset Date, string Type, string Desc, decimal Debit, decimal Credit)>();
        foreach (var inv in invoices)
        {
            lines.Add((inv.CreatedAt, "Invoice", $"Invoice Payment - #{inv.Id}", inv.Total, 0m));
        }

        foreach (var tx in transactions)
        {
            lines.Add((tx.Date, "Transaction", tx.Description, tx.AmountOut, tx.AmountIn));
        }

        lines.Sort((a, b) => a.Date.CompareTo(b.Date));

        var running = previousBalance;
        var result = new List<ClientStatementLineDto>();
        foreach (var line in lines)
        {
            running = running + line.Debit - line.Credit;
            result.Add(new ClientStatementLineDto(
                line.Type,
                line.Date.ToString("yyyy-MM-dd"),
                line.Desc,
                line.Debit,
                line.Credit,
                running));
        }

        return Result<ClientStatementDto>.Success(
            new ClientStatementDto(query.ClientId, clientName, previousBalance, result, running));
    }
}
