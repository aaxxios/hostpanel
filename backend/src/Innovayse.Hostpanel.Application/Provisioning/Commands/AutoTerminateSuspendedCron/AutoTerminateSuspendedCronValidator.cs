using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Provisioning.Commands.AutoTerminateSuspendedCron;

/// <summary>Validates <see cref="AutoTerminateSuspendedCronCommand"/>: no rules — the command carries no input.</summary>
public sealed class AutoTerminateSuspendedCronValidator : LocalizedValidator<AutoTerminateSuspendedCronCommand>
{
    /// <summary>Initializes the validator with the localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public AutoTerminateSuspendedCronValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
