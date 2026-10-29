using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Integration.Tests.Shared;

/// <summary>Pins the page arithmetic every list endpoint relies on.</summary>
public sealed class PaginatedListTests
{
    /// <summary>150 items at 20 per page round up to 8 pages, not 7.</summary>
    [Fact]
    public void TotalPagesRoundsUp()
    {
        var page = new PaginatedList<int>([], totalCount: 150, pageNumber: 1, pageSize: 20);

        page.TotalPages.Should().Be(8);
    }

    /// <summary>
    /// The first page never skips a row. The reason <see cref="PageOffset"/> exists is the
    /// overflow that <c>(pageNumber - 1) * pageSize</c> hits for a huge page, not this case, but
    /// page 1 is the floor its clamp promises.
    /// </summary>
    [Fact]
    public void OffsetForFirstPageIsZero()
    {
        PageOffset.For(pageNumber: 1, pageSize: 20).Should().Be(0);
    }

    /// <summary>
    /// A page number of <see cref="int.MaxValue"/> would overflow <see cref="int"/> under the naive
    /// formula and wrap negative; <see cref="PageOffset.For"/> computes in <see cref="long"/> and
    /// clamps to <see cref="int.MaxValue"/> instead, which is the exact promise its doc comment makes.
    /// </summary>
    [Fact]
    public void OffsetForMaxPageNumberClampsInsteadOfOverflowing()
    {
        var offset = PageOffset.For(pageNumber: int.MaxValue, pageSize: 20);

        offset.Should().Be(int.MaxValue);
        offset.Should().BeGreaterThanOrEqualTo(0);
    }
}
