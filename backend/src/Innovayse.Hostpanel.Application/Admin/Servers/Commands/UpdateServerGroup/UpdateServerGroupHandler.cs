using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Admin.Servers.Commands.UpdateServerGroup;

/// <summary>
/// Handles <see cref="UpdateServerGroupCommand"/> by updating the group and resyncing server
/// membership.
/// </summary>
public sealed class UpdateServerGroupHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public UpdateServerGroupHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Updates the group name and fill type, then resyncs which servers belong to it. Servers
    /// removed from the list have their group cleared; new servers are assigned.
    /// </summary>
    /// <param name="cmd">The update command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>SERVER_GROUP_NOT_FOUND</c> when the id names no group.</returns>
    public async Task<Result> HandleAsync(UpdateServerGroupCommand cmd, CancellationToken ct)
    {
        var group = await _context.ServerGroups
            .Include(g => g.Servers)
            .FirstOrDefaultAsync(g => g.Id == cmd.Id, ct);
        if (group is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.SERVER_GROUP_NOT_FOUND);
        }

        group.Update(cmd.Name, cmd.FillType);

        // Remove servers no longer in the group.
        foreach (var server in group.Servers.Where(s => !cmd.ServerIds.Contains(s.Id)).ToList())
        {
            server.RemoveFromGroup();
        }

        // Add newly assigned servers. A server named in the list but not found is silently
        // skipped -- the same shape the pre-conversion handler used.
        var currentIds = group.Servers.Select(s => s.Id).ToHashSet();
        var newIds = cmd.ServerIds.Where(id => !currentIds.Contains(id)).ToList();
        var newServers = await _context.Servers
            .Where(s => newIds.Contains(s.Id))
            .ToListAsync(ct);

        foreach (var server in newServers)
        {
            server.AssignToGroup(group.Id);
        }

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
