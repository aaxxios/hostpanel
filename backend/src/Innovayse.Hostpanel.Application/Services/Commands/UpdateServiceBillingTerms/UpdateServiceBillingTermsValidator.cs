using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Services.Commands.UpdateServiceBillingTerms;

/// <summary>Validates <see cref="UpdateServiceBillingTermsCommand"/> before the handler executes.</summary>
public sealed class UpdateServiceBillingTermsValidator : LocalizedValidator<UpdateServiceBillingTermsCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateServiceBillingTermsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ServiceId).MustBePositiveId(L["ServiceIdInvalid"]);
        RuleFor(x => x.ProductId).MustBePositiveId(L["ServiceProductIdInvalid"]);
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(1).WithMessage(L["ServiceQuantityInvalid"]);
        RuleFor(x => x.BillingCycle).IsInEnum().WithMessage(L["ServiceBillingCycleInvalid"]);
        RuleFor(x => x.FirstPaymentAmount).GreaterThanOrEqualTo(0).WithMessage(L["ServiceFirstPaymentAmountInvalid"]);
        RuleFor(x => x.RecurringAmount).GreaterThanOrEqualTo(0).WithMessage(L["ServiceRecurringAmountInvalid"]);
        RuleFor(x => x.PaymentMethod).MaximumLength(50).WithMessage(L["ServicePaymentMethodInvalid"])
            .When(x => x.PaymentMethod is not null);
        RuleFor(x => x.PromotionCode).MaximumLength(50).WithMessage(L["ServicePromotionCodeInvalid"])
            .When(x => x.PromotionCode is not null);
        RuleFor(x => x.SubscriptionId).MaximumLength(256).WithMessage(L["ServiceSubscriptionIdInvalid"])
            .When(x => x.SubscriptionId is not null);
    }
}
