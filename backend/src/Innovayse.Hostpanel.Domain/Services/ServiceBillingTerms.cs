using Innovayse.Hostpanel.Domain.Products.Enums;

namespace Innovayse.Hostpanel.Domain.Services;

/// <summary>
/// What a <see cref="ClientService"/> is billed: the cycle, quantity, amounts, the next renewal date and
/// the payment references. Staff change them together on the service's billing section, and
/// <see cref="ClientService.ChangeBillingTerms"/> applies them as one change.
/// </summary>
/// <param name="BillingCycle">The billing cycle.</param>
/// <param name="Quantity">Quantity ordered; at least one.</param>
/// <param name="FirstPaymentAmount">Amount charged on the first invoice; never negative.</param>
/// <param name="RecurringAmount">Amount charged on each renewal invoice; never negative.</param>
/// <param name="NextRenewalAt">Next renewal date, or <see langword="null"/> when the service does not renew.</param>
/// <param name="PaymentMethod">Payment method the service is paid with, or <see langword="null"/>.</param>
/// <param name="PromotionCode">Promotion code applied to the service, or <see langword="null"/>.</param>
/// <param name="SubscriptionId">External subscription reference, or <see langword="null"/>.</param>
public sealed record ServiceBillingTerms(
    BillingCycle BillingCycle,
    int Quantity,
    decimal FirstPaymentAmount,
    decimal RecurringAmount,
    DateTimeOffset? NextRenewalAt,
    string? PaymentMethod,
    string? PromotionCode,
    string? SubscriptionId);
