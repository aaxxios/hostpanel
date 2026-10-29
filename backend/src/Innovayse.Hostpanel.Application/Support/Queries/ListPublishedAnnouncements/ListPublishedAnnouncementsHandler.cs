using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Shared.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Queries.ListPublishedAnnouncements;

/// <summary>
/// Returns a paged list of published announcements, newest first, for the client portal.
/// </summary>
/// <remarks>
/// The published filter is applied inside the same query that pages, so the page numbers and
/// <c>TotalCount</c> describe the rows a client can actually see. Filtering a page after it had
/// been fetched would leave drafts occupying slots and make pages look short for no visible reason.
/// </remarks>
public sealed class ListPublishedAnnouncementsHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public ListPublishedAnnouncementsHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The paging window. It carries no visibility switch.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with the page of published announcements. This use case answers no refusal code.</returns>
    public async Task<Result<PaginatedList<PublishedAnnouncementDto>>> HandleAsync(
        ListPublishedAnnouncementsQuery query,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, PagingLimits.MinPageSize, PagingLimits.MaxPageSize);

        var baseQuery = _context.Announcements
            .AsNoTracking()
            .Where(a => a.IsPublished)
            .OrderByDescending(a => a.CreatedAt)
            .ThenByDescending(a => a.Id);

        var totalCount = await baseQuery.CountAsync(ct);
        var items = await baseQuery
            .Paginate(page, pageSize)
            .Select(a => new PublishedAnnouncementDto(a.Id, a.Title, a.Content, a.CreatedAt))
            .ToListAsync(ct);

        return Result.Success(new PaginatedList<PublishedAnnouncementDto>(items, totalCount, page, pageSize));
    }
}
