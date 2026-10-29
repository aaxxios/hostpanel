using Innovayse.Hostpanel.Infrastructure.Time;

namespace Innovayse.Hostpanel.Integration.Tests.Time;

/// <summary><see cref="SystemClock"/> reports whatever the registered time source says.</summary>
public sealed class SystemClockTests
{
    /// <summary>The clock reads the time source on every call, so moving the source moves the clock.</summary>
    [Fact]
    public void UtcNow_FollowsTheTimeSource()
    {
        var start = new DateTimeOffset(2026, 3, 1, 12, 0, 0, TimeSpan.Zero);
        var time = new ManualTimeProvider(start);
        var clock = new SystemClock(time);

        clock.UtcNow.Should().Be(start);

        time.Advance(TimeSpan.FromMinutes(5));

        clock.UtcNow.Should().Be(start.AddMinutes(5));
    }
}
