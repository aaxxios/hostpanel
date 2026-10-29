using Innovayse.Hostpanel.Application.Reports.Common.Enums;

namespace Innovayse.Hostpanel.Application.Reports.Common;

/// <summary>One group in the Customer Retention report.</summary>
/// <param name="Kind">Whether the group holds services of a product group or the registered domains.</param>
/// <param name="GroupName">Name of the product group; null for the domains group, whose label the client writes.</param>
/// <param name="Rows">Retention figures for each product and billing cycle in the group.</param>
public record CustomerRetentionGroupDto(
    RetentionGroupKind Kind,
    string? GroupName,
    IReadOnlyList<CustomerRetentionRowDto> Rows);
