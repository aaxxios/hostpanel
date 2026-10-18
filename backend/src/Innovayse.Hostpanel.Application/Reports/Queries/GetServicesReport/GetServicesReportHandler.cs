using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Shared.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Reports.Queries.GetServicesReport;

/// <summary>Handles <see cref="GetServicesReportQuery"/>.</summary>
public sealed class GetServicesReportHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetServicesReportHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The filters and page to return.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The matching page of services and the unfiltered total. Never refuses.</returns>
    public async Task<Result<ServiceReportResultDto>> HandleAsync(GetServicesReportQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var services = _context.ClientServices.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            services = services.Where(s => s.Status.ToString() == query.Status);
        }

        if (!string.IsNullOrWhiteSpace(query.BillingCycle))
        {
            services = services.Where(s => s.BillingCycle.ToLower() == query.BillingCycle.ToLower());
        }

        if (query.CreatedFrom.HasValue)
        {
            services = services.Where(s => s.CreatedAt >= query.CreatedFrom.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        }

        if (query.CreatedTo.HasValue)
        {
            services = services.Where(s => s.CreatedAt <= query.CreatedTo.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc));
        }

        if (query.NextDueFrom.HasValue)
        {
            services = services.Where(s => s.NextRenewalAt >= query.NextDueFrom.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        }

        if (query.NextDueTo.HasValue)
        {
            services = services.Where(s => s.NextRenewalAt <= query.NextDueTo.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc));
        }

        if (query.TerminatedFrom.HasValue)
        {
            services = services.Where(s => s.TerminatedAt >= query.TerminatedFrom.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        }

        if (query.TerminatedTo.HasValue)
        {
            services = services.Where(s => s.TerminatedAt <= query.TerminatedTo.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc));
        }

        var totalCount = await services.CountAsync(ct);

        // Names are resolved for every filtered row up front, not just the current page — the
        // pre-conversion repository did the same, so a page further in still shows real names.
        var clientIds = await services.Select(s => s.ClientId).Distinct().ToListAsync(ct);
        var clientNames = await _context.Clients.AsNoTracking()
            .Where(c => clientIds.Contains(c.Id))
            .Select(c => new { c.Id, Name = c.FirstName + " " + c.LastName })
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        var productIds = await services.Select(s => s.ProductId).Distinct().ToListAsync(ct);
        var productNames = await _context.Products.AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .Select(p => new { p.Id, p.Name })
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        var items = await services
            .OrderByDescending(s => s.CreatedAt)
            .Paginate(query.Page, query.PageSize)
            .ToListAsync(ct);

        var result = items.Select(s => new ServiceReportDto(
            s.Id,
            s.ClientId,
            clientNames.GetValueOrDefault(s.ClientId, string.Empty),
            s.ProductId,
            productNames.GetValueOrDefault(s.ProductId, string.Empty),
            s.Domain,
            s.BillingCycle,
            s.FirstPaymentAmount,
            s.RecurringAmount,
            s.PaymentMethod,
            s.CreatedAt.ToString("yyyy-MM-dd"),
            s.NextRenewalAt?.ToString("yyyy-MM-dd"),
            s.TerminatedAt?.ToString("yyyy-MM-dd"),
            s.Status.ToString(),
            s.AdminNotes)).ToList();

        return Result<ServiceReportResultDto>.Success(new ServiceReportResultDto(result, totalCount));
    }
}
