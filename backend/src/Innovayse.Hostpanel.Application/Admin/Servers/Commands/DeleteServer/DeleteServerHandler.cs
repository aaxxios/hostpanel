using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Admin.Servers.Commands.DeleteServer;

/// <summary>
/// Handles <see cref="DeleteServerCommand"/> by removing the server from the database.
/// </summary>
public sealed class DeleteServerHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public DeleteServerHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Deletes the server.
    /// </summary>
    /// <param name="cmd">The server to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>SERVER_NOT_FOUND</c> when the id names no server.</returns>
    public async Task<Result> HandleAsync(DeleteServerCommand cmd, CancellationToken ct)
    {
        var server = await _context.Servers.FirstOrDefaultAsync(s => s.Id == cmd.Id, ct);
        if (server is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.SERVER_NOT_FOUND);
        }

        _context.Servers.Remove(server);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
