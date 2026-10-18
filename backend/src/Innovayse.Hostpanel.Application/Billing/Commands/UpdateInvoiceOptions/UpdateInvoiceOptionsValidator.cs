using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Billing.Commands.UpdateInvoiceOptions;

/// <summary>Validates <see cref="UpdateInvoiceOptionsCommand"/>.</summary>
public sealed class UpdateInvoiceOptionsValidator : LocalizedValidator<UpdateInvoiceOptionsCommand>
{
    /// <summary>Maximum length of the payment method label.</summary>
    private const int PaymentMethodMaxLength = 100;

    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateInvoiceOptionsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.InvoiceId).MustBePositiveId(L["InvoiceIdInvalid"]);
        RuleFor(x => x.TaxRate).InclusiveBetween(0, 100).WithMessage(L["InvoiceTaxRateInvalid"]);
        RuleFor(x => x.PaymentMethod)
            .MaximumLength(PaymentMethodMaxLength)
            .When(x => x.PaymentMethod is not null)
            .WithMessage(L["InvoicePaymentMethodTooLong", PaymentMethodMaxLength]);
    }
}
