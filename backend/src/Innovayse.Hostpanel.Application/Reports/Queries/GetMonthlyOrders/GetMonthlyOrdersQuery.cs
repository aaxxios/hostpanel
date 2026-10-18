namespace Innovayse.Hostpanel.Application.Reports.Queries.GetMonthlyOrders;

/// <summary>Query for the Monthly Orders report, grouped by product group.</summary>
/// <param name="Year">Calendar year, or null for the current UTC year.</param>
/// <param name="Month">Calendar month 1-12, or null for the current UTC month.</param>
public sealed record GetMonthlyOrdersQuery(int? Year = null, int? Month = null);
