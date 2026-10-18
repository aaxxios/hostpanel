using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Shared.Extensions;

/// <summary>
/// Page slicing for list queries. Every paged list query composes its window through
/// <see cref="Paginate{T}"/> so the overflow-safe offset arithmetic of <see cref="PageOffset"/>
/// lives in one place instead of being re-typed as <c>(page - 1) * pageSize</c> in each handler.
/// </summary>
public static class QueryablePagingExtensions
{
    /// <summary>Restricts a query to one 1-based page of rows.</summary>
    /// <typeparam name="T">The row type.</typeparam>
    /// <param name="source">The ordered query to slice.</param>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Number of rows per page.</param>
    /// <returns>The query skipping the preceding pages and taking at most <paramref name="pageSize"/> rows.</returns>
    public static IQueryable<T> Paginate<T>(this IQueryable<T> source, int page, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Skip(PageOffset.For(page, pageSize)).Take(pageSize);
    }
}
