using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Billing.Commands.ApplyInvoiceCredit;

/// <summary>Validates <see cref="ApplyInvoiceCreditCommand"/>.</summary>
public sealed class ApplyInvoiceCreditValidator : LocalizedValidator<ApplyInvoiceCreditCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ApplyInvoiceCreditValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.InvoiceId).MustBePositiveId(L["InvoiceIdInvalid"]);
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage(L["InvoiceCreditAmountInvalid"]);
    }
}
