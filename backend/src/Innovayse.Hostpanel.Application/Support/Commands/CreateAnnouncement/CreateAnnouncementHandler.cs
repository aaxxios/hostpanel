using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Support;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Support.Commands.CreateAnnouncement;

/// <summary>Creates a new announcement.</summary>
public sealed class CreateAnnouncementHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public CreateAnnouncementHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The announcement to create.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with the new announcement's id. This use case answers no refusal code itself.</returns>
    public async Task<Result<int>> HandleAsync(CreateAnnouncementCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var announcement = Announcement.Create(cmd.Title, cmd.Content, cmd.IsPublished);
        _context.Announcements.Add(announcement);
        await _context.SaveChangesAsync(ct);

        return Result.Success(announcement.Id);
    }
}
