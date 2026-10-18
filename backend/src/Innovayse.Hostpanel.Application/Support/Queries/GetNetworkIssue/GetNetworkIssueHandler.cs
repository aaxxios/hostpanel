using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Application.Support.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Queries.GetNetworkIssue;

/// <summary>Loads a single network issue.</summary>
public sealed class GetNetworkIssueHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public GetNetworkIssueHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The network issue to read.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The matching issue, or <c>NETWORKISSUE_NOT_FOUND</c> when the id names no row.</returns>
    public async Task<Result<NetworkIssueDto>> HandleAsync(GetNetworkIssueQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var issue = await _context.NetworkIssues
            .AsNoTracking()
            .Where(n => n.Id == query.Id)
            .SelectDto()
            .FirstOrDefaultAsync(ct);

        return issue is null
            ? Result<NetworkIssueDto>.FailureCode(HostpanelErrorCodes.NETWORKISSUE_NOT_FOUND)
            : Result.Success(issue);
    }
}
