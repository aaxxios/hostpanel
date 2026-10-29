using Innovayse.Hostpanel.Domain.Products.Enums;

namespace Innovayse.Hostpanel.Application.Services.Common;

/// <summary>What a service costs, how often and how it is paid for.</summary>
/// <param name="Quantity">Quantity ordered.</param>
/// <param name="FirstPaymentAmount">First payment amount.</param>
/// <param name="RecurringAmount">Recurring charge amount.</param>
/// <param name="PaymentMethod">Payment method for this service.</param>
/// <param name="PromotionCode">Applied promotion code.</param>
/// <param name="SubscriptionId">External subscription reference.</param>
/// <param name="BillingCycle">The billing cycle.</param>
/// <param name="NextDueDate">Next renewal date, or <see langword="null"/> if not yet active.</param>
/// <param name="AutoRenew">Whether the billing runs look after the service; <see langword="false"/> means staff bill it by hand.</param>
public sealed record ServiceBillingTermsDto(
    int Quantity,
    decimal FirstPaymentAmount,
    decimal RecurringAmount,
    string? PaymentMethod,
    string? PromotionCode,
    string? SubscriptionId,
    BillingCycle BillingCycle,
    DateTimeOffset? NextDueDate,
    bool AutoRenew);
