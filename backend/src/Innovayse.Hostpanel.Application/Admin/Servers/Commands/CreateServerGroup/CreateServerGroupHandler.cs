using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Servers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Admin.Servers.Commands.CreateServerGroup;

/// <summary>
/// Handles <see cref="CreateServerGroupCommand"/> by creating the group and assigning servers.
/// </summary>
public sealed class CreateServerGroupHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public CreateServerGroupHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates the group, assigns requested servers, and returns the new group id.
    /// </summary>
    /// <param name="cmd">The create command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The new group's identifier. Always succeeds once validation has passed.</returns>
    public async Task<Result<int>> HandleAsync(CreateServerGroupCommand cmd, CancellationToken ct)
    {
        var group = ServerGroup.Create(cmd.Name, cmd.FillType);
        _context.ServerGroups.Add(group);
        await _context.SaveChangesAsync(ct);

        // A server named in the list but not found is silently skipped -- the same shape the
        // pre-conversion handler used, and asking for a server that does not exist is not treated
        // as a refusal of the group's own creation.
        var servers = await _context.Servers
            .Where(s => cmd.ServerIds.Contains(s.Id))
            .ToListAsync(ct);

        foreach (var server in servers)
        {
            server.AssignToGroup(group.Id);
        }

        await _context.SaveChangesAsync(ct);
        return Result<int>.Success(group.Id);
    }
}
