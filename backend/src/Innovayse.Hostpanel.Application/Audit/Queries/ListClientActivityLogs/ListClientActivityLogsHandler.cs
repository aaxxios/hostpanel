using System.Globalization;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Audit.Queries.ListClientActivityLogs;

/// <summary>Returns a paged, filtered list of activity log entries for a specific client.</summary>
public sealed class ListClientActivityLogsHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public ListClientActivityLogsHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles <see cref="ListClientActivityLogsQuery"/>.</summary>
    /// <param name="query">The query.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success with the page of activity log entries. A client id that names nobody simply
    /// answers an empty page; this use case refuses nothing.
    /// </returns>
    public async Task<Result<PagedResult<ActivityLogDto>>> HandleAsync(
        ListClientActivityLogsQuery query, CancellationToken ct)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, PagingLimits.MinPageSize, PagingLimits.MaxPageSize);

        // Was IActivityLogRepository.ListByClientIdAsync -- read-only, so no tracking is needed.
        var logsQuery = _context.ActivityLogs.AsNoTracking().Where(l => l.ClientId == query.ClientId);

        if (query.Date.HasValue)
        {
            var day = query.Date.Value.UtcDateTime.Date;
            var nextDay = day.AddDays(1);
            logsQuery = logsQuery.Where(l => l.CreatedAt >= day && l.CreatedAt < nextDay);
        }

        if (!string.IsNullOrWhiteSpace(query.AdminSearch))
        {
            // EF.Functions.Like is the provider-agnostic case-insensitive match this layer may
            // reference; ILike is Npgsql-specific and lives only in Infrastructure.
            var adminPattern = $"%{query.AdminSearch.ToLower(CultureInfo.InvariantCulture)}%";
            logsQuery = logsQuery.Where(l =>
                (l.AdminName != null && EF.Functions.Like(l.AdminName.ToLower(), adminPattern)) ||
                (l.AdminEmail != null && EF.Functions.Like(l.AdminEmail.ToLower(), adminPattern)));
        }

        if (!string.IsNullOrWhiteSpace(query.Description))
        {
            var descriptionPattern = $"%{query.Description.ToLower(CultureInfo.InvariantCulture)}%";
            logsQuery = logsQuery.Where(l => EF.Functions.Like(l.Description.ToLower(), descriptionPattern));
        }

        if (!string.IsNullOrWhiteSpace(query.IpAddress))
        {
            var ipPattern = $"%{query.IpAddress.ToLower(CultureInfo.InvariantCulture)}%";
            logsQuery = logsQuery.Where(l => l.IpAddress != null && EF.Functions.Like(l.IpAddress.ToLower(), ipPattern));
        }

        var total = await logsQuery.CountAsync(ct);
        var items = await logsQuery
            .OrderByDescending(l => l.CreatedAt)
            .Paginate(page, pageSize)
            .ToListAsync(ct);

        var dtos = items
            .Select(l => new ActivityLogDto(l.Id, l.Description, l.AdminName, l.AdminEmail, l.IpAddress, l.CreatedAt))
            .ToList();

        var result = new PagedResult<ActivityLogDto>(dtos, total, page, pageSize);
        return Result<PagedResult<ActivityLogDto>>.Success(result);
    }
}
