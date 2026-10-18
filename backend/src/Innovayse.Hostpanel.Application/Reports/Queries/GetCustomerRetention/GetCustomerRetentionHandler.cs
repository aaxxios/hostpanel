using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Domain.Domains.Enums;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Domain.Services;
using Innovayse.Hostpanel.Domain.Services.Enums;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Reports.Queries.GetCustomerRetention;

/// <summary>Handles <see cref="GetCustomerRetentionQuery"/>.</summary>
/// <remarks>
/// The average lifetime per product needs every matching row's span in scope at once, and the two
/// sources (services, domains) have unrelated shapes, so the final grouping and averaging stays
/// client-side exactly as the pre-conversion repository did it; only the two source queries move
/// into SQL.
/// </remarks>
public sealed class GetCustomerRetentionHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetCustomerRetentionHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">Whether services and domains still running count towards the average.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Average retention per product, grouped by product group. Never refuses.</returns>
    public async Task<Result<CustomerRetentionDto>> HandleAsync(GetCustomerRetentionQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var services = _context.ClientServices.AsNoTracking().AsQueryable();
        if (!query.IncludeActive)
        {
            services = services.Where(s => s.Status == ServiceStatus.Terminated);
        }

        var serviceRaw = await services
            .Join(_context.Products.AsNoTracking(), s => s.ProductId, p => p.Id, (s, p) => new { s, p })
            .Join(_context.ProductGroups.AsNoTracking(), x => x.p.GroupId, g => g.Id, (x, g) => new
            {
                GroupName = g.Name,
                ProductName = x.p.Name,
                BillingCycle = x.s.BillingCycle,
                CreatedAt = x.s.CreatedAt,
                EndAt = x.s.Status == ServiceStatus.Terminated ? x.s.TerminatedAt : x.s.NextRenewalAt,
            })
            .ToListAsync(ct);

        var domains = _context.Domains.AsNoTracking().AsQueryable();
        if (!query.IncludeActive)
        {
            domains = domains.Where(d => d.Status == DomainStatus.Expired || d.Status == DomainStatus.Cancelled);
        }

        var domainRaw = await domains
            .Select(d => new
            {
                GroupName = "TLD",
                ProductName = d.Tld,
                BillingCycle = "1 Years",
                CreatedAt = d.RegisteredAt,
                EndAt = (DateTimeOffset?)d.ExpiresAt,
            })
            .ToListAsync(ct);

        var allRows = serviceRaw
            .Select(r => new
            {
                r.GroupName,
                r.ProductName,
                r.BillingCycle,
                Days = r.EndAt.HasValue
                    ? (int)(r.EndAt.Value - r.CreatedAt).TotalDays
                    : (int)(DateTimeOffset.UtcNow - r.CreatedAt).TotalDays,
            })
            .Concat(domainRaw.Select(r => new
            {
                r.GroupName,
                r.ProductName,
                r.BillingCycle,
                Days = r.EndAt.HasValue
                    ? (int)(r.EndAt.Value - r.CreatedAt).TotalDays
                    : (int)(DateTimeOffset.UtcNow - r.CreatedAt).TotalDays,
            }))
            .ToList();

        var groups = allRows
            .GroupBy(r => r.GroupName)
            .Select(grp => new CustomerRetentionGroupDto(
                grp.Key,
                grp.GroupBy(r => new { r.ProductName, r.BillingCycle })
                    .Select(g =>
                    {
                        var avg = g.Any() ? (int)g.Average(r => r.Days) : 0;
                        return new CustomerRetentionRowDto(
                            g.Key.ProductName,
                            g.Key.BillingCycle,
                            g.Count(),
                            avg,
                            FormatYearsMonths(avg));
                    })
                    .OrderBy(r => r.ProductName)
                    .ToList()))
            .OrderBy(g => g.GroupName)
            .ToList();

        return Result<CustomerRetentionDto>.Success(new CustomerRetentionDto(groups));
    }

    /// <summary>Formats a day count as a human-friendly years/months/days span.</summary>
    /// <param name="days">Total whole days.</param>
    /// <returns>The largest two non-zero units, or a plain day count under one month.</returns>
    private static string FormatYearsMonths(int days)
    {
        var years = days / 365;
        var months = (days % 365) / 30;
        if (years > 0 && months > 0)
        {
            return $"{years} Years {months} Months";
        }

        if (years > 0)
        {
            return $"{years} Years";
        }

        if (months > 0)
        {
            return $"{months} Months";
        }

        return $"{days} Days";
    }
}
