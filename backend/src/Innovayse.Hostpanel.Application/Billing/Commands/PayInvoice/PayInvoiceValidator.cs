using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Billing.Commands.PayInvoice;

/// <summary>
///     Validator for <see cref="PayInvoiceCommand"/>. Checks only the shape of the id; whether it
///     names a payable invoice is the handler's answer.
/// </summary>
public sealed class PayInvoiceValidator : LocalizedValidator<PayInvoiceCommand>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public PayInvoiceValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.InvoiceId).GreaterThan(0);
    }
}
