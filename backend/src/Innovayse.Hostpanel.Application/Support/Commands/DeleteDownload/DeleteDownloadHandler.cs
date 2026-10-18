using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Commands.DeleteDownload;

/// <summary>Deletes a download entry.</summary>
public sealed class DeleteDownloadHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public DeleteDownloadHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The download to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>DOWNLOAD_NOT_FOUND</c> when the id names no download.</returns>
    public async Task<Result> HandleAsync(DeleteDownloadCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var download = await _context.Downloads.FirstOrDefaultAsync(d => d.Id == cmd.Id, ct);
        if (download is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.DOWNLOAD_NOT_FOUND);
        }

        _context.Downloads.Remove(download);
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
