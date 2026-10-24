using FluentAssertions;
using Innovayse.Hostpanel.Domain.Support;
using Xunit;
namespace Innovayse.Hostpanel.Domain.Tests.Support;

/// <summary>Unit tests for the <see cref="Announcement"/> entity.</summary>
public sealed class AnnouncementTests
{
    /// <summary>Backdate moves the creation date to the day the announcement was written and keeps its text and published state.</summary>
    [Fact]
    public void Backdate_MovesTheCreationDateOnly()
    {
        var announcement = Announcement.Create("Maintenance", "Tonight", isPublished: true);
        var written = new DateTimeOffset(2026, 5, 4, 9, 0, 0, TimeSpan.Zero);

        announcement.Backdate(written);

        announcement.CreatedAt.Should().Be(written);
        announcement.Title.Should().Be("Maintenance");
        announcement.IsPublished.Should().BeTrue();
    }
}
