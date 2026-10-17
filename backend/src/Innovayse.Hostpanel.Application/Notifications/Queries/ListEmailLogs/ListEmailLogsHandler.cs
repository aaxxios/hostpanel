using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Notifications.Common;
using Innovayse.Hostpanel.Application.Notifications.Extensions;
using Innovayse.Hostpanel.Shared.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Notifications.Queries.ListEmailLogs;

/// <summary>Returns a page of email log entries ordered by send time descending.</summary>
public sealed class ListEmailLogsHandler
{
    /// <summary>Fewest items a page may hold, however the caller asked.</summary>
    private const int MinPageSize = 1;

    /// <summary>Most items a page may hold, however the caller asked.</summary>
    private const int MaxPageSize = 100;

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public ListEmailLogsHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The requested page and page size.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The page of log entries, newest first. Never refuses.</returns>
    public async Task<Result<IReadOnlyList<EmailLogDto>>> HandleAsync(
        ListEmailLogsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, MinPageSize, MaxPageSize);

        var logs = await _context.EmailLogs
            .AsNoTracking()
            .OrderByDescending(l => l.SentAt)
            .Paginate(page, pageSize)
            .SelectDto()
            .ToListAsync(ct);

        return Result<IReadOnlyList<EmailLogDto>>.Success(logs);
    }
}
