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
    /// <param name="now">The current moment, from the clock.</param>
    /// <returns>Midnight UTC today.</returns>
    internal static DateTimeOffset Today(DateTimeOffset now)
    {
        return new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);
    }

    /// <summary>The first month, counted back from the current one, a customer is billed in; later joiners have shorter histories.</summary>
    /// <param name="position">The customer's position in the id order, the demo client first.</param>
    /// <returns>Months back, at most <see cref="HistoryMonths"/> minus one.</returns>
    internal static int FirstBillingMonthBack(int position) =>
        position == 0 ? HistoryMonths - 1 : Math.Min(HistoryMonths - 1, 3 + ((position * 5) % 9));

    /// <summary>
    /// The day a customer signed up: three weeks before the first invoice, so nothing in the customer's history
    /// (services, domains, orders, invoices, quotes, mail) is dated before the account existed.
    /// </summary>
    /// <param name="position">The customer's position in the id order, the demo client first.</param>
    /// <param name="today">The anchor.</param>
    /// <returns>The signup moment.</returns>
    internal static DateTimeOffset SignupDate(int position, DateTimeOffset today) =>
        today.AddMonths(-FirstBillingMonthBack(position)).AddDays(-(21 + (position % 7))).AddHours(9 + (position % 8));

    /// <summary>A fresh generator on the fixed seed, so each seeder draws the same sequence every time.</summary>
    /// <returns>A generator seeded with <see cref="RandomSeed"/>.</returns>
    internal static Random NewRandom() => new(RandomSeed);
}
