namespace Innovayse.Hostpanel.Infrastructure.Demo;

/// <summary>
/// The one moment every demo date is measured from, and the one random seed every demo figure is drawn from.
/// Seeding captures the anchor once and hands it to each seeder, so a row's date is "anchor minus a fixed
/// number of days" and a figure is the n-th draw of a fixed-seed generator: the picture is identical after
/// every reset, only shifted forward to today, so the charts always show the last twelve months.
/// </summary>
internal static class DemoAnchor
{
    /// <summary>The seed of every generator the demo seeders create; never changes, so figures repeat.</summary>
    internal const int RandomSeed = 20260101;

    /// <summary>Months of history the billing charts are filled for, the current month included.</summary>
    internal const int HistoryMonths = 12;

    /// <summary>Reads the anchor: midnight UTC at the start of the current day.</summary>
    /// <returns>Midnight UTC today.</returns>
    internal static DateTimeOffset Today()
    {
        var now = DateTimeOffset.UtcNow;
        return new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);
    }

    /// <summary>A fresh generator on the fixed seed, so each seeder draws the same sequence every time.</summary>
    /// <returns>A generator seeded with <see cref="RandomSeed"/>.</returns>
    internal static Random NewRandom() => new(RandomSeed);
}
