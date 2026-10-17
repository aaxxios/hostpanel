namespace Innovayse.Hostpanel.Application.Migration.Importers.Records;

/// <summary>One client service as the source plugin describes it.</summary>
/// <param name="ClientEmail">The client email.</param>
/// <param name="ProductName">The product name.</param>
/// <param name="BillingCycle">The billing cycle.</param>
/// <param name="Status">The status.</param>
/// <param name="Domain">The domain.</param>
/// <param name="Username">The username.</param>
/// <param name="NextDueDate">The next due date.</param>
/// <param name="TerminatedAt">The terminated at.</param>
/// <param name="SubscriptionId">The subscription identifier.</param>
/// <param name="AdminNotes">The admin notes.</param>
/// <param name="RecurringAmount">The recurring amount.</param>
/// <param name="FirstPaymentAmount">The first payment amount.</param>
/// <param name="PaymentMethod">The payment method.</param>
internal sealed record ServiceRecord(
    string ClientEmail, string ProductName, string BillingCycle, string Status,
    string? Domain, string? Username, DateTimeOffset? NextDueDate,
    DateTimeOffset? TerminatedAt, string? SubscriptionId, string? AdminNotes,
    decimal RecurringAmount, decimal FirstPaymentAmount, string? PaymentMethod);
