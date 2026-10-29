using Innovayse.Hostpanel.Application.Billing.Extensions;
using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Reports.Common;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Reports.Queries.TopClientsByIncome;

/// <summary>Handles <see cref="TopClientsByIncomeQuery"/>.</summary>
public sealed class TopClientsByIncomeHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>The bus the ranked clients' names are asked through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="bus">The bus the ranked clients' names are asked through.</param>
    public TopClientsByIncomeHandler(IHostpanelDbContext context, IMessageBus bus)
    {
        _context = context;
        _bus = bus;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">How many clients to return.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The clients with the highest net invoice income, highest first, in the base currency.
    /// Credit top-ups and credit refunds are not income and are left out. Never refuses.
    /// </returns>
    /// <exception cref="InvalidOperationException">An invoice names a client that does not exist, so the database is broken.</exception>
    public async Task<Result<IReadOnlyList<TopClientByIncomeDto>>> HandleAsync(
        TopClientsByIncomeQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        // The database sums per client and currency; the sums are converted before they are
        // ranked, since a client's dollars and another's drams cannot be compared as they stand.
        var sums = await _context.IncomeEntries()
            .GroupBy(e => new { e.ClientId, e.Currency })
            .Select(g => new
            {
                g.Key.ClientId,
                g.Key.Currency,
                AmountIn = g.Sum(e => e.Amount > 0 ? e.Amount : 0m),
                AmountOut = g.Sum(e => e.Amount < 0 ? -e.Amount : 0m),
                Fees = g.Sum(e => e.Fees),
            })
            .ToListAsync(ct);
        var rates = await _context.GetBaseCurrencyRatesAsync(ct);

        var top = sums
            .GroupBy(s => s.ClientId)
            .Select(g =>
            {
                var amountIn = g.Sum(s => rates.ToBase(s.AmountIn, s.Currency));
                var amountOut = g.Sum(s => rates.ToBase(s.AmountOut, s.Currency));
                var fees = g.Sum(s => rates.ToBase(s.Fees, s.Currency));
                return new { ClientId = g.Key, AmountIn = amountIn, AmountOut = amountOut, Fees = fees, Balance = amountIn - amountOut - fees };
            })
            .OrderByDescending(x => x.Balance)
            .ThenBy(x => x.ClientId)
            .Take(query.Take)
            .ToList();

        // FK_Invoices_Clients_ClientId restricts deleting a client that has invoices, and every
        // income entry comes from an invoice, so every ranked client is there.
        var ids = top.Select(x => x.ClientId).ToList();
        var clients = await _bus.GetRequiredClientBriefsByIdAsync(ids, ct);

        IReadOnlyList<TopClientByIncomeDto> rows = [.. top.Select(x => new TopClientByIncomeDto(
            x.ClientId,
            clients[x.ClientId].FullName,
            x.AmountIn,
            x.Fees,
            x.AmountOut,
            x.Balance,
            rates.Currency))];

        return Result<IReadOnlyList<TopClientByIncomeDto>>.Success(rows);
    }
}
