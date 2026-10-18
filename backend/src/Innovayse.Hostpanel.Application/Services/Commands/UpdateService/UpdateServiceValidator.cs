using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Innovayse.Hostpanel.Domain.Products.Enums;
using Innovayse.Hostpanel.Domain.Services.Enums;
using Microsoft.Extensions.Localization;
namespace Innovayse.Hostpanel.Application.Services.Commands.UpdateService;
/// <summary>Validates <see cref="UpdateServiceCommand"/> before the handler executes.</summary>
public sealed class UpdateServiceValidator : LocalizedValidator<UpdateServiceCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateServiceValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ServiceId).MustBePositiveId(L["ServiceIdInvalid"]);
        RuleFor(x => x.Domain).MaximumLength(253).WithMessage(L["UpdateServiceDomainInvalid"])
            .When(x => x.Domain is not null);
        RuleFor(x => x.DedicatedIp).MaximumLength(45).WithMessage(L["UpdateServiceDedicatedIpInvalid"])
            .When(x => x.DedicatedIp is not null);
        RuleFor(x => x.Username).MaximumLength(100).WithMessage(L["UpdateServiceUsernameInvalid"])
            .When(x => x.Username is not null);
        RuleFor(x => x.Password).MaximumLength(256).WithMessage(L["UpdateServicePasswordInvalid"])
            .When(x => x.Password is not null);
        RuleFor(x => x.BillingCycle).NotEmpty().MaximumLength(20)
            .WithMessage(L["UpdateServiceBillingCycleInvalid"]);
        RuleFor(x => x.RecurringAmount).GreaterThanOrEqualTo(0)
            .WithMessage(L["UpdateServiceRecurringAmountInvalid"]);
        RuleFor(x => x.PaymentMethod).MaximumLength(50).WithMessage(L["UpdateServicePaymentMethodInvalid"])
            .When(x => x.PaymentMethod is not null);
        RuleFor(x => x.SubscriptionId).MaximumLength(256).WithMessage(L["UpdateServiceSubscriptionIdInvalid"])
            .When(x => x.SubscriptionId is not null);
        RuleFor(x => x.AutoTerminateReason).MaximumLength(2000)
            .WithMessage(L["UpdateServiceAutoTerminateReasonInvalid"])
            .When(x => x.AutoTerminateReason is not null);
        RuleFor(x => x.AdminNotes).MaximumLength(2000).WithMessage(L["UpdateServiceAdminNotesInvalid"])
            .When(x => x.AdminNotes is not null);
        RuleFor(x => x.ProvisioningRef).MaximumLength(256).WithMessage(L["UpdateServiceProvisioningRefInvalid"])
            .When(x => x.ProvisioningRef is not null);
        RuleFor(x => x.FirstPaymentAmount).GreaterThanOrEqualTo(0)
            .WithMessage(L["UpdateServiceFirstPaymentAmountInvalid"]);
        RuleFor(x => x.PromotionCode).MaximumLength(50).WithMessage(L["UpdateServicePromotionCodeInvalid"])
            .When(x => x.PromotionCode is not null);
        RuleFor(x => x.Status)
            .Must(s => Enum.TryParse<ServiceStatus>(s, ignoreCase: true, out _))
            .WithMessage(L["UpdateServiceStatusInvalid"])
            .When(x => x.Status is not null);
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(1).WithMessage(L["UpdateServiceQuantityInvalid"]);
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage(L["UpdateServiceProductIdInvalid"])
            .When(x => x.ProductId is not null);
        RuleFor(x => x.ServerId).GreaterThan(0).WithMessage(L["UpdateServiceServerIdInvalid"])
            .When(x => x.ServerId is not null);
    }
}
