using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Billing.Commands.ProcessRenewalsCron;

/// <summary>Validates <see cref="ProcessRenewalsCronCommand"/>: no rules — the command carries no input.</summary>
public sealed class ProcessRenewalsCronValidator : LocalizedValidator<ProcessRenewalsCronCommand>
{
    /// <summary>Initializes the validator with the localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ProcessRenewalsCronValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
