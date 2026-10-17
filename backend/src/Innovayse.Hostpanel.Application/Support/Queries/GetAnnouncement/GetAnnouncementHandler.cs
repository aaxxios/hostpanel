using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Application.Support.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Queries.GetAnnouncement;

/// <summary>Loads a single announcement.</summary>
public sealed class GetAnnouncementHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetAnnouncementHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The announcement to read.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The matching announcement, or <c>ANNOUNCEMENT_NOT_FOUND</c> when the id names no row.</returns>
    public async Task<Result<AnnouncementDto>> HandleAsync(GetAnnouncementQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var announcement = await _context.Announcements
            .AsNoTracking()
            .Where(a => a.Id == query.Id)
            .SelectDto()
            .FirstOrDefaultAsync(ct);

        return announcement is null
            ? Result<AnnouncementDto>.FailureCode(HostpanelErrorCodes.ANNOUNCEMENT_NOT_FOUND)
            : Result.Success(announcement);
    }
}
