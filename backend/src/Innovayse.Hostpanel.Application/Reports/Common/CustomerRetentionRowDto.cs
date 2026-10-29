using Innovayse.Hostpanel.Domain.Products.Enums;

namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>One row in the Customer Retention report.</summary>
/// <param name="ProductName">Product (or top-level domain) the retention figures were measured for.</param>
/// <param name="BillingCycle">Billing cycle of a service; null for a domain, which has a term instead.</param>
/// <param name="TermYears">Registration term in years of a domain; null for a service.</param>
/// <param name="ProductCount">Number of services the average was taken over.</param>
/// <param name="AvgDaysActive">Mean number of days a service stayed active.</param>
/// <param name="AvgYears">Whole years in the average (365 days each).</param>
/// <param name="AvgMonths">Whole months (30 days each) left after the years.</param>
public record CustomerRetentionRowDto(
    string ProductName,
    BillingCycle? BillingCycle,
    int? TermYears,
    int ProductCount,
    int AvgDaysActive,
    int AvgYears,
    int AvgMonths);
