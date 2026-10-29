using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Billing.Commands.CheckOverdueInvoicesCron;

/// <summary>Validates <see cref="CheckOverdueInvoicesCronCommand"/>: no rules; the command carries no input.</summary>
public sealed class CheckOverdueInvoicesCronValidator : LocalizedValidator<CheckOverdueInvoicesCronCommand>
{
    /// <summary>Initializes the validator with the localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public CheckOverdueInvoicesCronValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
