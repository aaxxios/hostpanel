using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Commands.UpdateAnnouncement;

/// <summary>Updates an existing announcement's title, content, and published state.</summary>
public sealed class UpdateAnnouncementHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public UpdateAnnouncementHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The fields to update.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>ANNOUNCEMENT_NOT_FOUND</c> when the id names no announcement.</returns>
    public async Task<Result> HandleAsync(UpdateAnnouncementCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var announcement = await _context.Announcements.FirstOrDefaultAsync(a => a.Id == cmd.Id, ct);
        if (announcement is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.ANNOUNCEMENT_NOT_FOUND);
        }

        announcement.Update(cmd.Title, cmd.Content, cmd.IsPublished);
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
