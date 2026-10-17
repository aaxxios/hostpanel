using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Admin.Servers.Commands.DeleteServerGroup;

/// <summary>
/// Handles <see cref="DeleteServerGroupCommand"/> by removing the group. Servers in the group are
/// unassigned by the foreign key's own cascade rule.
/// </summary>
public sealed class DeleteServerGroupHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public DeleteServerGroupHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Deletes the group.
    /// </summary>
    /// <param name="cmd">The group to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>SERVER_GROUP_NOT_FOUND</c> when the id names no group.</returns>
    public async Task<Result> HandleAsync(DeleteServerGroupCommand cmd, CancellationToken ct)
    {
        var group = await _context.ServerGroups.FirstOrDefaultAsync(g => g.Id == cmd.Id, ct);
        if (group is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.SERVER_GROUP_NOT_FOUND);
        }

        _context.ServerGroups.Remove(group);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
