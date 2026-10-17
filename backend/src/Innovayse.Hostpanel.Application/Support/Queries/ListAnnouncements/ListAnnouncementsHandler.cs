using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Application.Support.Extensions;
using Innovayse.Hostpanel.Shared.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Queries.ListAnnouncements;

/// <summary>Returns a paged list of announcements ordered by creation date descending.</summary>
public sealed class ListAnnouncementsHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public ListAnnouncementsHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The paging window.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with the page. This use case answers no refusal code.</returns>
    public async Task<Result<PagedResult<AnnouncementDto>>> HandleAsync(ListAnnouncementsQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var baseQuery = _context.Announcements.AsNoTracking().OrderByDescending(a => a.CreatedAt);

        var totalCount = await baseQuery.CountAsync(ct);
        var items = await baseQuery
            .Paginate(page, pageSize)
            .SelectDto()
            .ToListAsync(ct);

        return Result.Success(new PagedResult<AnnouncementDto>(items, totalCount, page, pageSize));
    }
}
