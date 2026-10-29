using Innovayse.Hostpanel.Domain.Hosting;

namespace Innovayse.Hostpanel.Domain.Tests.Hosting;

/// <summary>Unit tests for <see cref="DiskUsageStat"/>.</summary>
public sealed class DiskUsageStatTests
{
    /// <summary>The moment the first figures were fetched.</summary>
    private static readonly DateTimeOffset FetchedAt = new(2026, 10, 1, 8, 0, 0, TimeSpan.Zero);

    /// <summary>Create records the account, its usage figures and when they were fetched.</summary>
    [Fact]
    public void Create_RecordsTheUsage()
    {
        var stat = DiskUsageStat.Create("web-1", "example.test", "Jane Doe", 100, 1000, 200, 5000, FetchedAt);

        stat.ServerName.Should().Be("web-1");
        stat.Domain.Should().Be("example.test");
        stat.ClientName.Should().Be("Jane Doe");
        stat.DiskUsageMb.Should().Be(100);
        stat.DiskLimitMb.Should().Be(1000);
        stat.BwUsageMb.Should().Be(200);
        stat.BwLimitMb.Should().Be(5000);
        stat.UpdatedAt.Should().Be(FetchedAt);
    }

    /// <summary>A refresh replaces all four figures and stamps the time of the refresh.</summary>
    [Fact]
    public void RecordUsage_ReplacesTheFigures()
    {
        var stat = DiskUsageStat.Create("web-1", "example.test", "Jane Doe", 100, 1000, 200, 5000, FetchedAt);
        var refreshedAt = FetchedAt.AddHours(6);

        stat.RecordUsage(150, 2000, 300, 6000, refreshedAt);

        stat.DiskUsageMb.Should().Be(150);
        stat.DiskLimitMb.Should().Be(2000);
        stat.BwUsageMb.Should().Be(300);
        stat.BwLimitMb.Should().Be(6000);
        stat.UpdatedAt.Should().Be(refreshedAt);
    }
}
