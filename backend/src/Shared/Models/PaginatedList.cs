using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Innovayse.Hostpanel.Shared.Models;

/// <summary>
///     Generic paginated list for API responses.
///     Contains items and pagination metadata.
/// </summary>
/// <typeparam name="T">The type of the items contained in the page.</typeparam>
[SuppressMessage(
    "Design",
    "CA1000:Do not declare static members on generic types",
    Justification = "House API: PaginatedList<T>.CreateAsync is how list queries build a page.")]
[SuppressMessage("Design", "CA1002:Do not expose generic lists", Justification = "House API: Items is a List<T>.")]
public class PaginatedList<T>
{
    /// <summary>
    ///     Initializes a new paginated list
    /// </summary>
    /// <param name="items">Items on current page</param>
    /// <param name="totalCount">Total count of items across all pages</param>
    /// <param name="pageNumber">Current page number</param>
    /// <param name="pageSize">Number of items per page</param>
    public PaginatedList(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageSize = pageSize;
        PageNumber = pageNumber;

        // Adaptation: a zero or negative page size cannot divide; such a list has no pages.
        TotalPages = pageSize <= 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
    }

    /// <summary>Items on current page</summary>
    public List<T> Items { get; }

    /// <summary>Current page number (1-based)</summary>
    public int PageNumber { get; }

    /// <summary>Items per page</summary>
    public int PageSize { get; }

    /// <summary>Total number of pages</summary>
    public int TotalPages { get; }

    /// <summary>Total items across all pages</summary>
    public int TotalCount { get; }

    /// <summary>Whether previous page exists</summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>Whether next page exists</summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>Optional extra data about the list as a whole</summary>
    public object? Metadata { get; set; }

    /// <summary>
    ///     Create paginated list from IQueryable source.
    ///     Executes count and page queries.
    /// </summary>
    /// <param name="source">IQueryable source to paginate</param>
    /// <param name="pageNumber">Page number to retrieve</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list with items and metadata</returns>
    public static async Task<PaginatedList<T>> CreateAsync(
        IQueryable<T> source,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var count = await source.CountAsync(cancellationToken);

        var items = await source
            .Skip(PageOffset.For(pageNumber, pageSize))
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }
}
