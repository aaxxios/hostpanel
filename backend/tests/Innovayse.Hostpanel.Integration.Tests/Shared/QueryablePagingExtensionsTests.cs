using FluentAssertions;
using Innovayse.Hostpanel.Shared.Extensions;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Shared;

/// <summary>Pins the page window every paged list query takes through <see cref="QueryablePagingExtensions.Paginate{T}"/>.</summary>
public sealed class QueryablePagingExtensionsTests
{
    /// <summary>Gets the ten rows the tests page over.</summary>
    private static IQueryable<int> Rows => Enumerable.Range(1, 10).AsQueryable();

    /// <summary>The first page holds the first rows.</summary>
    [Fact]
    public void Paginate_FirstPage_TakesTheLeadingRows()
    {
        Rows.Paginate(page: 1, pageSize: 3).Should().Equal(1, 2, 3);
    }

    /// <summary>A later page skips the earlier pages.</summary>
    [Fact]
    public void Paginate_SecondPage_SkipsTheFirstPage()
    {
        Rows.Paginate(page: 2, pageSize: 3).Should().Equal(4, 5, 6);
    }

    /// <summary>The last page may be short.</summary>
    [Fact]
    public void Paginate_LastPage_HoldsTheRemainder()
    {
        Rows.Paginate(page: 4, pageSize: 3).Should().Equal(10);
    }

    /// <summary>A page past the end is empty rather than an error, even for the largest page number.</summary>
    /// <param name="page">The page number asked for.</param>
    [Theory]
    [InlineData(5)]
    [InlineData(int.MaxValue)]
    public void Paginate_PastTheEnd_IsEmpty(int page)
    {
        Rows.Paginate(page, pageSize: 3).Should().BeEmpty();
    }
}
