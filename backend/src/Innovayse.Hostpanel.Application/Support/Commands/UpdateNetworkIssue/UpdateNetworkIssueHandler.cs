using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Commands.UpdateNetworkIssue;

/// <summary>Updates an existing network issue.</summary>
public sealed class UpdateNetworkIssueHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public UpdateNetworkIssueHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The fields to update. Validated upstream.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success, or <c>NETWORKISSUE_NOT_FOUND</c> when the id names no network issue.</returns>
    public async Task<Result> HandleAsync(UpdateNetworkIssueCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var issue = await _context.NetworkIssues.FirstOrDefaultAsync(n => n.Id == cmd.Id, ct);
        if (issue is null)
        {
            return Result.FailureCode(HostpanelErrorCodes.NETWORKISSUE_NOT_FOUND);
        }

        issue.Update(cmd.Title, cmd.Type, cmd.Server, cmd.Priority, cmd.Status, cmd.StartDate, cmd.EndDate, cmd.Description);
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
