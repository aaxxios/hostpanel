namespace Innovayse.Hostpanel.Shared.Infrastructure;

/// <summary>
/// Date math for jobs that re-schedule themselves once a day at a fixed UTC hour. Every daily cron
/// handler asks this class for its next run instead of carrying its own copy of the arithmetic.
/// </summary>
public static class DailySchedule
{
    /// <summary>Calculates the next UTC occurrence of the given hour.</summary>
    /// <param name="hour">Target hour in UTC (0-23).</param>
    /// <param name="now">The current moment; the caller supplies it so the result is deterministic under test.</param>
    /// <returns>
    /// Today at <paramref name="hour"/>:00 UTC when that moment is still ahead of
    /// <paramref name="now"/>; otherwise the same hour tomorrow.
    /// </returns>
    public static DateTimeOffset NextOccurrence(int hour, DateTimeOffset now)
    {
        var candidate = new DateTimeOffset(now.Year, now.Month, now.Day, hour, 0, 0, TimeSpan.Zero);
        return candidate > now ? candidate : candidate.AddDays(1);
    }
}
