using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Billing.Commands.SuspendOverdueServicesCron;

/// <summary>Validates <see cref="SuspendOverdueServicesCronCommand"/>: no rules — the command carries no input.</summary>
public sealed class SuspendOverdueServicesCronValidator : LocalizedValidator<SuspendOverdueServicesCronCommand>
{
    /// <summary>Initializes the validator with the localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public SuspendOverdueServicesCronValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
