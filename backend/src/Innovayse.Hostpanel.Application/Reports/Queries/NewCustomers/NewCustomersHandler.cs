using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Reports.Queries.NewCustomers;

/// <summary>Handles <see cref="NewCustomersQuery"/>.</summary>
public sealed class NewCustomersHandler
{
    /// <summary>Three-letter month abbreviations, index 0 is January.</summary>
    private static readonly string[] MonthNames =
        ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public NewCustomersHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The year to report on; omitted means the current UTC year.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Twelve rows, January first, with signups, orders and the conversion rate. Never refuses.</returns>
    public async Task<Result<IReadOnlyList<NewCustomerDto>>> HandleAsync(NewCustomersQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var year = query.Year ?? DateTime.UtcNow.Year;

        var newClients = await _context.Clients.AsNoTracking()
            .Where(c => c.CreatedAt.Year == year)
            .GroupBy(c => c.CreatedAt.Month)
            .Select(g => new { Month = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var orders = await _context.Orders.AsNoTracking()
            .Where(o => o.CreatedAt.Year == year)
            .GroupBy(o => o.CreatedAt.Month)
            .Select(g => new { Month = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        IReadOnlyList<NewCustomerDto> rows = Enumerable.Range(1, 12).Select(m =>
        {
            var signups = newClients.FirstOrDefault(x => x.Month == m)?.Count ?? 0;
            var placed = orders.FirstOrDefault(x => x.Month == m)?.Count ?? 0;
            var rate = signups > 0 ? (double)placed / signups * 100 : 0;
            return new NewCustomerDto(
                Month: MonthNames[m - 1],
                NewSignups: signups,
                OrdersPlaced: placed,
                OrdersCompleted: placed,
                ConversionRate: Math.Round(rate, 1));
        }).ToList();

        return Result<IReadOnlyList<NewCustomerDto>>.Success(rows);
    }
}
