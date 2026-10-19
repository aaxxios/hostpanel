using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Application.Support.Extensions;
using Innovayse.Hostpanel.Domain.Support;
using Innovayse.Hostpanel.Domain.Support.Enums;
using Innovayse.Hostpanel.Shared.Extensions;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Support.Queries.ListNetworkIssues;

/// <summary>Returns a paged list of network issues, optionally filtered by status.</summary>
public sealed class ListNetworkIssuesHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public ListNetworkIssuesHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The paging window and optional status filter.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success with the page. This use case answers no refusal code.</returns>
    public async Task<Result<PagedResult<NetworkIssueDto>>> HandleAsync(ListNetworkIssuesQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, PagingLimits.MinPageSize, PagingLimits.MaxPageSize);

        NetworkIssueStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(query.Status) &&
            Enum.TryParse<NetworkIssueStatus>(query.Status, ignoreCase: true, out var parsed))
        {
            statusFilter = parsed;
        }

        var baseQuery = _context.NetworkIssues
            .AsNoTracking()
            .Where(n => statusFilter == null || n.Status == statusFilter)
            .OrderByDescending(n => n.CreatedAt);

        var totalCount = await baseQuery.CountAsync(ct);
        var items = await baseQuery
            .Paginate(page, pageSize)
            .SelectDto()
            .ToListAsync(ct);

        return Result.Success(new PagedResult<NetworkIssueDto>(items, totalCount, page, pageSize));
    }
}
