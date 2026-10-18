using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Reports.Queries.GetDomainsReport;

/// <summary>Handles <see cref="GetDomainsReportQuery"/>.</summary>
public sealed class GetDomainsReportHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetDomainsReportHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The filters and page to return.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The matching page of domains and the unfiltered total. Never refuses.</returns>
    public async Task<Result<DomainReportResultDto>> HandleAsync(GetDomainsReportQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var domains = _context.Domains.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            domains = domains.Where(d => d.Status.ToString() == query.Status);
        }

        if (!string.IsNullOrWhiteSpace(query.Registrar))
        {
            domains = domains.Where(d => d.Registrar != null && d.Registrar.ToLower().Contains(query.Registrar.ToLower()));
        }

        if (query.RegisteredFrom.HasValue)
        {
            domains = domains.Where(d => d.RegisteredAt >= query.RegisteredFrom.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        }

        if (query.RegisteredTo.HasValue)
        {
            domains = domains.Where(d => d.RegisteredAt <= query.RegisteredTo.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc));
        }

        if (query.ExpiresFrom.HasValue)
        {
            domains = domains.Where(d => d.ExpiresAt >= query.ExpiresFrom.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        }

        if (query.ExpiresTo.HasValue)
        {
            domains = domains.Where(d => d.ExpiresAt <= query.ExpiresTo.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc));
        }

        if (query.NextDueFrom.HasValue)
        {
            domains = domains.Where(d => d.NextDueDate >= query.NextDueFrom.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        }

        if (query.NextDueTo.HasValue)
        {
            domains = domains.Where(d => d.NextDueDate <= query.NextDueTo.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc));
        }

        var totalCount = await domains.CountAsync(ct);

        // The pre-conversion repository joined the Identity Users table for this dictionary
        // without ever reading anything from it, which under AUTH_MODE=sso — where that table is
        // empty — silently blanked every client name in the report. The client's own name is on
        // the Client row already; the join added nothing but the bug. Dropped.
        var clientIds = await domains.Select(d => d.ClientId).Distinct().ToListAsync(ct);
        var clientNames = await _context.Clients
            .AsNoTracking()
            .Where(c => clientIds.Contains(c.Id))
            .Select(c => new { c.Id, Name = c.FirstName + " " + c.LastName })
            .ToDictionaryAsync(x => x.Id, x => x.Name, ct);

        var items = await domains
            .OrderByDescending(d => d.RegisteredAt)
            .Paginate(query.Page, query.PageSize)
            .Select(d => new
            {
                d.Id,
                d.ClientId,
                d.OrderId,
                d.OrderType,
                d.Name,
                d.FirstPaymentAmount,
                d.RecurringAmount,
                d.RegistrationPeriod,
                d.RegisteredAt,
                d.ExpiresAt,
                d.NextDueDate,
                d.Registrar,
                d.PaymentMethod,
                d.Status,
                d.AdminNotes,
            })
            .ToListAsync(ct);

        IReadOnlyList<DomainReportDto> result = items.Select(d => new DomainReportDto(
            d.Id,
            d.ClientId,
            clientNames.GetValueOrDefault(d.ClientId, string.Empty),
            d.OrderId,
            d.OrderType,
            d.Name,
            d.FirstPaymentAmount,
            d.RecurringAmount,
            d.RegistrationPeriod,
            d.RegisteredAt.ToString("yyyy-MM-dd"),
            d.ExpiresAt.ToString("yyyy-MM-dd"),
            d.NextDueDate.ToString("yyyy-MM-dd"),
            d.Registrar,
            d.PaymentMethod,
            d.Status.ToString(),
            d.AdminNotes)).ToList();

        return Result<DomainReportResultDto>.Success(new DomainReportResultDto(result, totalCount));
    }
}
