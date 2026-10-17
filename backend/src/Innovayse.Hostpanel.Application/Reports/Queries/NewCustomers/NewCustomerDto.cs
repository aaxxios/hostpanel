namespace Innovayse.Hostpanel.Application.Reports.Queries.NewCustomers;

/// <summary>One row of the New Customers report.</summary>
/// <param name="Month">The month.</param>
/// <param name="NewSignups">The new signups.</param>
/// <param name="OrdersPlaced">The orders placed.</param>
/// <param name="OrdersCompleted">The orders completed.</param>
/// <param name="ConversionRate">The conversion rate.</param>
public record NewCustomerDto(
    string Month,
    int NewSignups,
    int OrdersPlaced,
    int OrdersCompleted,
    double ConversionRate);
