namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>
/// The calendar month a monthly report covers: the requested year and month, each defaulting to the
/// current UTC one, and the half-open UTC window <c>[From, To)</c> that month spans. Every monthly
/// report resolves its period here instead of repeating the defaulting and window arithmetic.
/// </summary>
/// <param name="Year">The report year.</param>
/// <param name="Month">The report month, 1-12.</param>
public readonly record struct ReportMonth(int Year, int Month)
{
    /// <summary>Gets the first instant of the month (UTC), inclusive.</summary>
    public DateTimeOffset From => new(Year, Month, 1, 0, 0, 0, TimeSpan.Zero);

    /// <summary>Gets the first instant of the following month (UTC), exclusive.</summary>
    public DateTimeOffset To => From.AddMonths(1);

    /// <summary>Gets the number of days in the month.</summary>
    public int DaysInMonth => DateTime.DaysInMonth(Year, Month);

    /// <summary>Resolves the requested period, filling an omitted year or month from the clock.</summary>
    /// <param name="year">The requested year, or <c>null</c> for the current one.</param>
    /// <param name="month">The requested month, or <c>null</c> for the current one.</param>
    /// <param name="now">The current moment (UTC).</param>
    /// <returns>The period to report on.</returns>
    public static ReportMonth Resolve(int? year, int? month, DateTimeOffset now)
    {
        return new ReportMonth(year ?? now.Year, month ?? now.Month);
    }
}
