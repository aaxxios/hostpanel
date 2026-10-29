namespace Innovayse.Hostpanel.Shared.Models;

/// <summary>
///     Turns a page number and page size into the row offset a query should skip.
/// </summary>
/// <remarks>
///     The obvious <c>(pageNumber - 1) * pageSize</c> overflows <see cref="int" /> for a large page
///     number: <c>?pageNumber=2147483647</c> wraps to a negative value, which reaches Postgres as
///     <c>OFFSET -N</c> and surfaces as an unhandled 500. Computing in <see cref="long" /> and
///     clamping yields an empty page instead, which is the correct answer past the last page.
/// </remarks>
public static class PageOffset
{
    /// <summary>
    ///     Computes the number of rows to skip for a 1-based page, without overflowing.
    /// </summary>
    /// <param name="pageNumber">1-based page number.</param>
    /// <param name="pageSize">Number of rows per page.</param>
    /// <returns>The row offset, clamped to the non-negative <see cref="int" /> range.</returns>
    public static int For(int pageNumber, int pageSize)
    {
        var offset = (long)(pageNumber - 1) * pageSize;

        return offset <= 0 ? 0 : (int)Math.Min(offset, int.MaxValue);
    }
}
