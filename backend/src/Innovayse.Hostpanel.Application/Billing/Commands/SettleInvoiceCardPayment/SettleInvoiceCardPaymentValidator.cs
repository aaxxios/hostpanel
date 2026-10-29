using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Billing.Commands.SettleInvoiceCardPayment;

/// <summary>Validates <see cref="SettleInvoiceCardPaymentCommand"/>.</summary>
public sealed class SettleInvoiceCardPaymentValidator : LocalizedValidator<SettleInvoiceCardPaymentCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public SettleInvoiceCardPaymentValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.InvoiceId).MustBePositiveId(L["InvoiceIdInvalid"]);
        RuleFor(x => x.TransactionId).NotEmpty().WithMessage(L["TransactionIdInvalid"]);
    }
}
