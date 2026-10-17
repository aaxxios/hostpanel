using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Auth.Commands.StartTwoFactorSetup;

/// <summary>
///     Validator for StartTwoFactorSetupCommand. The command carries no input, so it declares no
///     rules; the absence was decided, not forgotten.
/// </summary>
public sealed class StartTwoFactorSetupValidator : LocalizedValidator<StartTwoFactorSetupCommand>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public StartTwoFactorSetupValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
