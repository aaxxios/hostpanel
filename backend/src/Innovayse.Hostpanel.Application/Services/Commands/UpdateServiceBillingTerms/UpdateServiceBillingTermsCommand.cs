using Innovayse.Hostpanel.Domain.Products.Enums;

namespace Innovayse.Hostpanel.Application.Services.Commands.UpdateServiceBillingTerms;

/// <summary>
/// Staff change what a service is billed: its product, quantity, cycle, amounts, next renewal date and
/// payment references, as saved from the billing section of the service page.
/// </summary>
/// <param name="ServiceId">The service primary key; the route id wins over any id in the body.</param>
/// <param name="ProductId">The product the service is of.</param>
/// <param name="Quantity">Quantity ordered; at least one.</param>
/// <param name="BillingCycle">The billing cycle.</param>
/// <param name="FirstPaymentAmount">Amount charged on the first invoice.</param>
/// <param name="RecurringAmount">Amount charged on each renewal invoice.</param>
/// <param name="NextRenewalAt">Next renewal date, or <see langword="null"/> to clear it.</param>
/// <param name="PaymentMethod">Payment method, or <see langword="null"/> to clear it.</param>
/// <param name="PromotionCode">Promotion code, or <see langword="null"/> to clear it.</param>
/// <param name="SubscriptionId">External subscription reference, or <see langword="null"/> to clear it.</param>
public sealed record UpdateServiceBillingTermsCommand(
    int ServiceId,
    int ProductId,
    int Quantity,
    BillingCycle BillingCycle,
    decimal FirstPaymentAmount,
    decimal RecurringAmount,
    DateTimeOffset? NextRenewalAt,
    string? PaymentMethod,
    string? PromotionCode,
    string? SubscriptionId);
