namespace Innovayse.Hostpanel.Application.Admin.Queries.GetServiceUsageReport;

/// <summary>
/// Represents the usage count of a specific product across all client services.
/// </summary>
/// <param name="ProductName">Display name of the product. <c>null</c> when it cannot be resolved; the caller renders its own fallback.</param>
/// <param name="Count">Number of active client service instances for this product.</param>
public record ServiceUsageReportItemDto(string? ProductName, int Count);
