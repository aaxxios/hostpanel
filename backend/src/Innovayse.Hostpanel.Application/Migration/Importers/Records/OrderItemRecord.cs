namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One line item on an <see cref="OrderRecord"/>.</summary>
/// <param name="ProductId">The product identifier.</param>
/// <param name="ProductName">The product name.</param>
/// <param name="BillingCycle">The billing cycle.</param>
/// <param name="FirstPaymentAmount">The first payment amount.</param>
/// <param name="RecurringAmount">The recurring amount.</param>
/// <param name="Domain">The domain.</param>
/// <param name="Hostname">The hostname.</param>
internal sealed record OrderItemRecord(int? ProductId, string ProductName, string BillingCycle, decimal FirstPaymentAmount, decimal RecurringAmount, string? Domain, string? Hostname);
