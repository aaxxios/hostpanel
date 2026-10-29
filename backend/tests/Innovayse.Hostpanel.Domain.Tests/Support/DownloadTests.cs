using Innovayse.Hostpanel.Domain.Support;

namespace Innovayse.Hostpanel.Domain.Tests.Support;

/// <summary>Unit tests for the <see cref="Download"/> entity.</summary>
public sealed class DownloadTests
{
    /// <summary>Backdate moves the creation date to the day the file was published and keeps the download count.</summary>
    [Fact]
    public void Backdate_MovesTheCreationDateOnly()
    {
        var download = Download.Create("Guide", "A guide", "PDF Document", "guide.pdf", categoryId: 1, clientsOnly: false, productDownload: false, isHidden: false);
        download.IncrementDownloadCount();
        var published = new DateTimeOffset(2026, 1, 20, 9, 0, 0, TimeSpan.Zero);

        download.Backdate(published);

        download.CreatedAt.Should().Be(published);
        download.DownloadCount.Should().Be(1);
    }
}
