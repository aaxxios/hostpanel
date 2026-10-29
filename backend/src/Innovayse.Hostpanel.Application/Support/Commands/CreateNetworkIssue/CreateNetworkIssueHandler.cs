using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Support;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Support.Commands.CreateNetworkIssue;

/// <summary>Creates a new network issue.</summary>
public sealed class CreateNetworkIssueHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public CreateNetworkIssueHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the command.</summary>
    /// <param name="cmd">The network issue to create. Validated upstream.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with the new issue's id. This use case answers no refusal code itself.</returns>
    public async Task<Result<int>> HandleAsync(CreateNetworkIssueCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var issue = NetworkIssue.Create(cmd.Title, cmd.Type, cmd.Server, cmd.Priority, cmd.StartDate, cmd.Description);

        _context.NetworkIssues.Add(issue);
        await _context.SaveChangesAsync(ct);

        return Result.Success(issue.Id);
    }
}
