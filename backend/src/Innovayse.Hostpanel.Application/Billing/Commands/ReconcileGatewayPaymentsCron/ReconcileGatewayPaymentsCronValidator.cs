using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Billing.Commands.ReconcileGatewayPaymentsCron;

/// <summary>Validates <see cref="ReconcileGatewayPaymentsCronCommand"/>: no rules; the command carries no input.</summary>
public sealed class ReconcileGatewayPaymentsCronValidator : LocalizedValidator<ReconcileGatewayPaymentsCronCommand>
{
    /// <summary>Initializes the validator with the localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ReconcileGatewayPaymentsCronValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
