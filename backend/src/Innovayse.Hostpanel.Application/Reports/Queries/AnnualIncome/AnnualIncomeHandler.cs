using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Reports.Queries.AnnualIncome;

/// <summary>Handles <see cref="AnnualIncomeQuery"/>.</summary>
public sealed class AnnualIncomeHandler
{
    /// <summary>Three-letter month names, indexed 0 for January.</summary>
    private static readonly string[] MonthNames =
        ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public AnnualIncomeHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The year to report on; defaults to the current UTC year.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Twelve rows, January first, zero-filled for months with no paid invoices. Never refuses.</returns>
    public async Task<Result<IReadOnlyList<AnnualIncomeDto>>> HandleAsync(AnnualIncomeQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var year = query.Year ?? DateTime.UtcNow.Year;

        var monthly = await _context.Invoices
            .AsNoTracking()
            .Where(i => i.Status == InvoiceStatus.Paid && i.CreatedAt.Year == year)
            .GroupBy(i => i.CreatedAt.Month)
            .Select(g => new { Month = g.Key, Total = g.Sum(i => i.Total) })
            .ToListAsync(ct);

        IReadOnlyList<AnnualIncomeDto> rows = Enumerable.Range(1, 12).Select(m => new AnnualIncomeDto(
            Month: MonthNames[m - 1],
            Amount: monthly.FirstOrDefault(x => x.Month == m)?.Total ?? 0m
        )).ToList();

        return Result<IReadOnlyList<AnnualIncomeDto>>.Success(rows);
    }
}
