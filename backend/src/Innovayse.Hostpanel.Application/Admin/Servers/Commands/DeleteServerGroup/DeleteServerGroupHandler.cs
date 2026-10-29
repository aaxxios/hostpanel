using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Products.Queries.IsServerGroupInUse;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace Innovayse.Hostpanel.Application.Admin.Servers.Commands.DeleteServerGroup;

/// <summary>
/// Handles <see cref="DeleteServerGroupCommand"/> by removing the group. Servers in the group are
/// unassigned by the foreign key's own cascade rule. A group that products still provision into is
/// refused: those products would silently lose the servers their orders are placed on.
/// </summary>
public sealed class DeleteServerGroupHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Wolverine bus the Products module's usage query is dispatched through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    /// <param name="bus">Wolverine bus the Products module's usage query is dispatched through.</param>
    public DeleteServerGroupHandler(IHostpanelDbContext context, IMessageBus bus)
    {
        _context = context;
        _bus = bus;
    }

    /// <summary>
    /// Deletes the group.
    /// </summary>
    /// <param name="cmd">The group to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Success, <c>SERVER_GROUP_NOT_FOUND</c> when the id names no group, or
    /// <c>SERVER_GROUP_IN_USE</c> when a product still references it.
    /// </returns>
    public async Task<Result> HandleAsync(DeleteServerGroupCommand cmd, CancellationToken ct)
    {
        var group = await _context.ServerGroups.FirstOrDefaultAsync(g => g.Id == cmd.Id, ct);
        if (group is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.SERVER_GROUP_NOT_FOUND);
        }

        var usage = await _bus.InvokeAsync<Result<bool>>(new IsServerGroupInUseQuery(cmd.Id), ct);
        if (usage.IsFailure)
        {
            return Result.FailureCode(usage.ErrorCode!);
        }

        if (usage.Value)
        {
            return Result.FailureCode(HostpanelErrorCodes.SERVER_GROUP_IN_USE);
        }

        _context.ServerGroups.Remove(group);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
