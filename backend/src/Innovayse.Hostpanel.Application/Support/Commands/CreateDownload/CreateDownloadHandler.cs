using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Support;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Support.Commands.CreateDownload;

/// <summary>Creates a new download entry.</summary>
public sealed class CreateDownloadHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public CreateDownloadHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The download's details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with the new download's id. This use case answers no refusal code.</returns>
    public async Task<Result<int>> HandleAsync(CreateDownloadCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var download = Download.Create(
            cmd.Title,
            cmd.Description,
            cmd.Type,
            cmd.Filename,
            cmd.CategoryId,
            cmd.ClientsOnly,
            cmd.ProductDownload,
            cmd.IsHidden);

        _context.Downloads.Add(download);
        await _context.SaveChangesAsync(ct);

        return Result.Success(download.Id);
    }
}
