using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Reports.Queries.GetSalesTaxReport;

/// <summary>Handles <see cref="GetSalesTaxReportQuery"/>.</summary>
public sealed class GetSalesTaxReportHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetSalesTaxReportHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The date range to report on; both bounds are optional.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Tax liability broken down as the report presents it. Never refuses.</returns>
    public async Task<Result<SalesTaxReportDto>> HandleAsync(GetSalesTaxReportQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var baseQuery = _context.Invoices.AsNoTracking().Where(i => i.Status == InvoiceStatus.Paid);

        if (query.From.HasValue)
        {
            var fromDt = new DateTimeOffset(query.From.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
            baseQuery = baseQuery.Where(i => i.CreatedAt >= fromDt);
        }

        if (query.To.HasValue)
        {
            var toDt = new DateTimeOffset(query.To.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc));
            baseQuery = baseQuery.Where(i => i.CreatedAt <= toDt);
        }

        var rows = await baseQuery
            .OrderBy(i => i.CreatedAt)
            .Join(_context.Clients, i => i.ClientId, c => c.Id, (i, c) => new SalesTaxRowDto(
                i.Id,
                c.FirstName + " " + c.LastName,
                i.CreatedAt.ToString("dd/MM/yyyy"),
                i.PaidAt != null ? i.PaidAt.Value.ToString("dd/MM/yyyy") : null,
                i.SubTotal,
                i.Tax,
                i.Credit,
                i.Total))
            .ToListAsync(ct);

        // Tax Level 2 is always zero — the pre-conversion report never used a second tax tier.
        var dto = new SalesTaxReportDto(rows.Count, rows.Sum(r => r.Total), rows.Sum(r => r.Tax), 0m, rows);
        return Result<SalesTaxReportDto>.Success(dto);
    }
}
