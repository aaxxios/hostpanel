namespace Innovayse.Hostpanel.Application.Domains.Common;

/// <summary>What a domain costs and how it is paid for.</summary>
/// <param name="FirstPaymentAmount">One-time registration cost.</param>
/// <param name="RecurringAmount">Recurring registration price.</param>
/// <param name="PriceCurrency">ISO 4217 currency code for the two amounts.</param>
/// <param name="PaymentMethod">Payment method label (e.g. "Credit/Debit Card"); null if not set.</param>
/// <param name="PromotionCode">Applied promotion/coupon code; null if none.</param>
/// <param name="SubscriptionId">External payment subscription reference; null if none.</param>
public sealed record DomainBillingDto(
    decimal FirstPaymentAmount,
    decimal RecurringAmount,
    string PriceCurrency,
    string? PaymentMethod,
    string? PromotionCode,
    string? SubscriptionId);
