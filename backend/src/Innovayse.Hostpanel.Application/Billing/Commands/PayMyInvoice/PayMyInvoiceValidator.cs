using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Billing.Commands.PayMyInvoice;

/// <summary>
/// Validator for <see cref="PayMyInvoiceCommand"/>. Checks only the shape of the id; whether it
/// names an invoice the caller may pay is the handler's answer.
/// </summary>
public sealed class PayMyInvoiceValidator : LocalizedValidator<PayMyInvoiceCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public PayMyInvoiceValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.InvoiceId).MustBePositiveId(L["InvoiceIdInvalid"]);
    }
}
