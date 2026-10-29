using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Queries.GetPublishedAnnouncement;

/// <summary>Loads one published announcement for the public detail page.</summary>
/// <remarks>
/// A draft and an id that names no row answer the same <c>ANNOUNCEMENT_NOT_FOUND</c>, so the route
/// cannot be used to discover unpublished announcements.
/// </remarks>
public sealed class GetPublishedAnnouncementHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetPublishedAnnouncementHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The announcement to read.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The published announcement, or <c>ANNOUNCEMENT_NOT_FOUND</c> when the id names no published row.</returns>
    public async Task<Result<PublishedAnnouncementDto>> HandleAsync(GetPublishedAnnouncementQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var announcement = await _context.Announcements
            .AsNoTracking()
            .Where(a => a.Id == query.Id && a.IsPublished)
            .Select(a => new PublishedAnnouncementDto(a.Id, a.Title, a.Content, a.CreatedAt))
            .FirstOrDefaultAsync(ct);

        return announcement is null
            ? Result<PublishedAnnouncementDto>.FailureCode(HostpanelErrorCodes.ANNOUNCEMENT_NOT_FOUND)
            : Result.Success(announcement);
    }
}
