using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Notifications.Common;
using Innovayse.Hostpanel.Application.Notifications.Extensions;
using Innovayse.Hostpanel.Shared.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Notifications.Queries.ListClientEmailLogs;

/// <summary>Returns a page of email logs sent to a specific client.</summary>
public sealed class ListClientEmailLogsHandler
{
    /// <summary>Fewest items a page may hold, however the caller asked.</summary>
    private const int MinPageSize = PagingLimits.MinPageSize;

    /// <summary>Most items a page may hold, however the caller asked.</summary>
    private const int MaxPageSize = PagingLimits.MaxPageSize;

    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Resolves the client's sign-in address; a log entry is keyed by address, not by id.</summary>
    private readonly IIdentityProvider _identity;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="identity">Resolves the client's sign-in address.</param>
    public ListClientEmailLogsHandler(IHostpanelDbContext context, IIdentityProvider identity)
    {
        _context = context;
        _identity = identity;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The client, requested page and page size.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The client's page of log entries, newest first, or an empty page when the client id names
    /// nobody -- the same answer <c>ListByClientIdAsync</c> gave before this conversion, never a
    /// refusal.
    /// </returns>
    public async Task<Result<PagedResult<EmailLogDto>>> HandleAsync(
        ListClientEmailLogsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, MinPageSize, MaxPageSize);

        var client = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == query.ClientId, ct);
        var account = client is null ? null : await _identity.FindBySubjectAsync(client.UserId, ct);
        if (account is null)
        {
            return Result<PagedResult<EmailLogDto>>.Success(new PagedResult<EmailLogDto>([], 0, page, pageSize));
        }

        var baseQuery = _context.EmailLogs.AsNoTracking().Where(l => l.To == account.Email);
        var total = await baseQuery.CountAsync(ct);
        var items = await baseQuery
            .OrderByDescending(l => l.SentAt)
            .Paginate(page, pageSize)
            .SelectDto()
            .ToListAsync(ct);

        return Result<PagedResult<EmailLogDto>>.Success(new PagedResult<EmailLogDto>(items, total, page, pageSize));
    }
}
