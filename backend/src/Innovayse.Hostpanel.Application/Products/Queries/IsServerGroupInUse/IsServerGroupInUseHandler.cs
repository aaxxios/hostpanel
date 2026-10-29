using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Products.Queries.IsServerGroupInUse;

/// <summary>
/// Handles <see cref="IsServerGroupInUseQuery"/>: a group is in use while a product names it as the
/// group its orders are provisioned into.
/// </summary>
public sealed class IsServerGroupInUseHandler
{
    /// <summary>The module's data context.</summary>
    private readonly IHostpanelDbContext _context;

    /// <summary>Initializes the handler.</summary>
    /// <param name="context">The module's data context.</param>
    public IsServerGroupInUseHandler(IHostpanelDbContext context)
    {
        _context = context;
    }

    /// <summary>Handles the query.</summary>
    /// <param name="query">The group to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><see langword="true"/> when a product references the group. Never refuses.</returns>
    public async Task<Result<bool>> HandleAsync(IsServerGroupInUseQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var inUse = await _context.Products.AsNoTracking().AnyAsync(p => p.ServerGroupId == query.ServerGroupId, ct);

        return Result<bool>.Success(inUse);
    }
}
