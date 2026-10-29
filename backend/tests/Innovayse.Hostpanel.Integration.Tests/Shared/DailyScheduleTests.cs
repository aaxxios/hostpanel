using Innovayse.Hostpanel.Shared.Infrastructure;

namespace Innovayse.Hostpanel.Integration.Tests.Shared;

/// <summary>Pins the next-run arithmetic every daily cron handler re-schedules itself with.</summary>
public sealed class DailyScheduleTests
{
    /// <summary>Before the target hour, the next run is later the same day.</summary>
    [Fact]
    public void NextOccurrence_BeforeTheHour_IsTodayAtTheHour()
    {
        var now = new DateTimeOffset(2026, 3, 10, 5, 30, 0, TimeSpan.Zero);

        var next = DailySchedule.NextOccurrence(hour: 7, now);

        next.Should().Be(new DateTimeOffset(2026, 3, 10, 7, 0, 0, TimeSpan.Zero));
    }

    /// <summary>At exactly the target hour the run has already started, so the next one is tomorrow.</summary>
    [Fact]
    public void NextOccurrence_AtTheHour_IsTomorrowAtTheHour()
    {
        var now = new DateTimeOffset(2026, 3, 10, 7, 0, 0, TimeSpan.Zero);

        var next = DailySchedule.NextOccurrence(hour: 7, now);

        next.Should().Be(new DateTimeOffset(2026, 3, 11, 7, 0, 0, TimeSpan.Zero));
    }

    /// <summary>After the target hour, the next run is tomorrow at that hour.</summary>
    [Fact]
    public void NextOccurrence_AfterTheHour_IsTomorrowAtTheHour()
    {
        var now = new DateTimeOffset(2026, 3, 10, 23, 59, 0, TimeSpan.Zero);

        var next = DailySchedule.NextOccurrence(hour: 4, now);

        next.Should().Be(new DateTimeOffset(2026, 3, 11, 4, 0, 0, TimeSpan.Zero));
    }

    /// <summary>Tomorrow rolls the month over correctly at the end of a month.</summary>
    [Fact]
    public void NextOccurrence_AtMonthEnd_RollsIntoTheNextMonth()
    {
        var now = new DateTimeOffset(2026, 1, 31, 9, 0, 0, TimeSpan.Zero);

        var next = DailySchedule.NextOccurrence(hour: 6, now);

        next.Should().Be(new DateTimeOffset(2026, 2, 1, 6, 0, 0, TimeSpan.Zero));
    }
}
