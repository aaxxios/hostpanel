namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>One month of the Income Forecast report; every amount is in <paramref name="Currency"/>.</summary>
/// <param name="Month">Month label, as the annual income report names it.</param>
/// <param name="Monthly">Income booked in this month alone.</param>
/// <param name="Quarterly">Income over this month and the two before it.</param>
/// <param name="SemiAnnual">Income over this month and the five before it.</param>
/// <param name="Annual">Income over the whole year, repeated on every row for comparison.</param>
/// <param name="Total">Income from January up to and including this month.</param>
/// <param name="Currency">ISO 4217 code of the base currency the amounts are converted to.</param>
public sealed record IncomeForecastRowDto(
    string Month,
    decimal Monthly,
    decimal Quarterly,
    decimal SemiAnnual,
    decimal Annual,
    decimal Total,
    string Currency);
