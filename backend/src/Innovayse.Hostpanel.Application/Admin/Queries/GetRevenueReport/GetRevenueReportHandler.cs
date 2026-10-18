using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
namespace Innovayse.Hostpanel.Application.Admin.Queries.GetRevenueReport;

/// <summary>
/// Handles <see cref="GetRevenueReportQuery"/> by grouping paid invoices by payment date.
/// </summary>
public sealed class GetRevenueReportHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetRevenueReportHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Returns daily revenue totals for the requested date range.
    /// </summary>
    /// <param name="query">The revenue report query with start and end dates.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success with the daily revenue items, ordered by date. A start after the end simply
    /// answers an empty report; this use case refuses nothing.
    /// </returns>
    public async Task<Result<IReadOnlyList<RevenueReportItemDto>>> HandleAsync(
        GetRevenueReportQuery query, CancellationToken ct)
    {
        // Was IInvoiceRepository.GetPaidBetweenAsync -- read-only, so no tracking is needed.
        var invoices = await _context.Invoices
            .AsNoTracking()
            .Where(i => i.Status == InvoiceStatus.Paid && i.PaidAt >= query.StartDate && i.PaidAt <= query.EndDate)
            .OrderBy(i => i.PaidAt)
            .ToListAsync(ct);

        IReadOnlyList<RevenueReportItemDto> items = invoices
            .GroupBy(i => DateOnly.FromDateTime(i.PaidAt!.Value.UtcDateTime))
            .Select(g => new RevenueReportItemDto(g.Key, g.Sum(i => i.Total)))
            .OrderBy(r => r.Date)
            .ToList();

        return Result<IReadOnlyList<RevenueReportItemDto>>.Success(items);
    }
}
