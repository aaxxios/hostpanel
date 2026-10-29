using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Services.Queries.IsServerInUse;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Admin.Servers.Commands.DeleteServer;

/// <summary>
/// Handles <see cref="DeleteServerCommand"/> by removing the server from the database. A server that
/// still hosts client services or SSH keys is refused: those rows would be left pointing at nothing.
/// </summary>
public sealed class DeleteServerHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Wolverine bus the Services module's usage query is dispatched through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="bus">Wolverine bus the Services module's usage query is dispatched through.</param>
    public DeleteServerHandler(IHostpanelDbContext context, IMessageBus bus)
    {
        _context = context;
        _bus = bus;
    }

    /// <summary>
    /// Deletes the server.
    /// </summary>
    /// <param name="cmd">The server to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, <c>SERVER_NOT_FOUND</c> when the id names no server, or <c>SERVER_IN_USE</c> when a
    /// client service or an SSH key still references it.
    /// </returns>
    public async Task<Result> HandleAsync(DeleteServerCommand cmd, CancellationToken ct)
    {
        var server = await _context.Servers.FirstOrDefaultAsync(s => s.Id == cmd.Id, ct);
        if (server is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.SERVER_NOT_FOUND);
        }

        var usage = await _bus.InvokeAsync<Result<bool>>(new IsServerInUseQuery(cmd.Id), ct);
        if (usage.IsFailure)
        {
            return Result.FailureCode(usage.ErrorCode!);
        }

        if (usage.Value)
        {
            return Result.FailureCode(HostpanelErrorCodes.SERVER_IN_USE);
        }

        _context.Servers.Remove(server);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
