using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Reports.Queries.TopClientsByIncome;

/// <summary>Handles <see cref="TopClientsByIncomeQuery"/>.</summary>
public sealed class TopClientsByIncomeHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public TopClientsByIncomeHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">How many clients to return.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The top clients by transaction income, highest first. Never refuses.</returns>
    public async Task<Result<IReadOnlyList<TopClientByIncomeDto>>> HandleAsync(
        TopClientsByIncomeQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var rows = await _context.Transactions.AsNoTracking()
            .GroupBy(t => t.ClientId)
            .Select(g => new
            {
                ClientId = g.Key,
                TotalIn = g.Sum(t => t.AmountIn),
                TotalFees = g.Sum(t => t.Fees),
                TotalOut = g.Sum(t => t.AmountOut),
            })
            .OrderByDescending(x => x.TotalIn)
            .Take(query.Take)
            .Join(_context.Clients, x => x.ClientId, c => c.Id,
                (x, c) => new TopClientByIncomeDto(
                    x.ClientId,
                    c.FirstName + " " + c.LastName,
                    x.TotalIn,
                    x.TotalFees,
                    x.TotalOut,
                    x.TotalIn - x.TotalOut - x.TotalFees))
            .ToListAsync(ct);

        return Result<IReadOnlyList<TopClientByIncomeDto>>.Success(rows);
    }
}
