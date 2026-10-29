using Innovayse.Hostpanel.Application.Reports.Common.Enums;

namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>One period row of the Aging Invoices summary report.</summary>
/// <param name="Period">The aging bucket the row totals.</param>
/// <param name="AmountsByCurrency">The amounts by currency.</param>
public record AgingPeriodDto(
    AgingBucket Period,
    Dictionary<string, decimal> AmountsByCurrency);
