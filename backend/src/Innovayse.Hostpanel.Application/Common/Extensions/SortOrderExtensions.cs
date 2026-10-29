using Innovayse.Hostpanel.SharedKernel.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Application.Common.Extensions;

/// <summary>Applies an operator's reorder request to any set of sortable rows.</summary>
public static class SortOrderExtensions
{
    /// <summary>
    /// Loads the rows a reorder request names and moves each through its own
    /// <see cref="ISortable.SetSortOrder"/>. Rows the request does not name keep their position. When an
    /// id appears twice, its last entry wins. If any id names no row, nothing is moved and the request
    /// is refused, because a partly applied reorder would leave the list in an order nobody chose.
    /// </summary>
    /// <remarks>
    /// Two operators reordering the same list at once cannot interleave their changes: each entity type
    /// carries the <c>xmin</c> concurrency token, so the second save of a row the first one changed fails
    /// with a concurrency conflict (answered 409) instead of silently overwriting it. The caller saves,
    /// in one transaction.
    /// </remarks>
    /// <typeparam name="T">The sortable entity.</typeparam>
    /// <param name="rows">The table the request reorders.</param>
    /// <param name="items">The id and new position pairs, as sent.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><see langword="true"/> when every named row was moved; <see langword="false"/> when an id names no row and nothing was moved.</returns>
    public static async Task<bool> ApplySortOrderAsync<T>(
        this IQueryable<T> rows, IReadOnlyCollection<SortOrderItem> items, CancellationToken ct)
        where T : class, ISortable
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(items);

        var orderMap = items.GroupBy(i => i.Id).ToDictionary(g => g.Key, g => g.Last().SortOrder);
        var ids = orderMap.Keys.ToList();
        var tracked = await rows.Where(x => ids.Contains(x.Id)).ToListAsync(ct);

        if (tracked.Count != ids.Count)
        {
            return false;
        }

        foreach (var row in tracked)
        {
            row.SetSortOrder(orderMap[row.Id]);
        }

        return true;
    }
}
