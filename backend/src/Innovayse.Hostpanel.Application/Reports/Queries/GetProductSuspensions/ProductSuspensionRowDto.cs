namespace Innovayse.Hostpanel.Application.Reports.Queries.GetProductSuspensions;

/// <summary>One row in the Product Suspensions report.</summary>
/// <param name="ServiceId">The service identifier.</param>
/// <param name="ClientName">The client name.</param>
/// <param name="ProductName">The product name.</param>
/// <param name="Domain">The domain.</param>
/// <param name="NextDueDate">The next due date.</param>
/// <param name="SuspendReason">The suspend reason.</param>
public record ProductSuspensionRowDto(
    int ServiceId,
    string ClientName,
    string ProductName,
    string? Domain,
    string? NextDueDate,
    string? SuspendReason);
