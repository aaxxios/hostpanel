using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Reports.Common;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Domain.Products;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Reports.Queries.GetIncomeByProductGrouped;

/// <summary>Handles <see cref="GetIncomeByProductGroupedQuery"/>.</summary>
public sealed class GetIncomeByProductGroupedHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetIncomeByProductGroupedHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The month to report on; defaults to the current UTC year and month.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paid-invoice income for the month, grouped by active product group. Never refuses.</returns>
    public async Task<Result<IncomeByProductGroupedDto>> HandleAsync(
        GetIncomeByProductGroupedQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var period = ReportMonth.Resolve(query.Year, query.Month, DateTimeOffset.UtcNow);
        var year = period.Year;
        var month = period.Month;
        var from = period.From;
        var to = period.To;

        // Income from paid invoices this month, matched by product name.
        var paidItems = await _context.InvoiceItems
            .AsNoTracking()
            .Join(
                _context.Invoices.AsNoTracking().Where(i => i.Status == InvoiceStatus.Paid && i.CreatedAt >= from && i.CreatedAt < to),
                item => item.InvoiceId, inv => inv.Id, (item, inv) => item)
            .GroupBy(item => item.Description)
            .Select(g => new { Name = g.Key, Units = g.Sum(x => x.Quantity), Income = g.Sum(x => x.UnitPrice * x.Quantity) })
            .ToListAsync(ct);

        var incomeMap = paidItems.ToDictionary(x => x.Name, x => new { x.Units, x.Income });

        var groups = await _context.ProductGroups
            .AsNoTracking()
            .Where(g => g.IsActive)
            .Include(g => g.Products.Where(p => p.Status == ProductStatus.Active))
            .OrderBy(g => g.Name)
            .ToListAsync(ct);

        var result = groups.Select(g =>
        {
            var products = g.Products.Select(p =>
            {
                var sale = incomeMap.GetValueOrDefault(p.Name);
                return new IncomeByProductRowDto(p.Id, p.Name, sale?.Units ?? 0, sale?.Income ?? 0m);
            }).ToList();

            return new IncomeByProductGroupDto(
                g.Name,
                products,
                products.Sum(p => p.UnitsSold),
                products.Sum(p => p.TotalIncome));
        }).ToList();

        var dto = new IncomeByProductGroupedDto(
            month, year, result,
            result.Sum(g => g.GroupUnitsSold),
            result.Sum(g => g.GroupIncome));

        return Result<IncomeByProductGroupedDto>.Success(dto);
    }
}
