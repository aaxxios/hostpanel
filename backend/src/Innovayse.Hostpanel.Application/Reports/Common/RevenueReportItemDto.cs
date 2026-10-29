namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>
/// A single data point in the revenue report: one UTC day's income, payments less refunds,
/// converted to the base currency.
/// </summary>
/// <param name="Date">The UTC calendar day of this data point.</param>
/// <param name="Revenue">Income of the day in <paramref name="Currency"/>.</param>
/// <param name="Currency">ISO 4217 code of the base currency <paramref name="Revenue"/> is in.</param>
public sealed record RevenueReportItemDto(DateOnly Date, decimal Revenue, string Currency);
