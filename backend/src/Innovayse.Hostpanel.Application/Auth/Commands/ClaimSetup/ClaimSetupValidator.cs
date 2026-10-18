using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Auth.Commands.ClaimSetup;

/// <summary>
/// Validator for <see cref="ClaimSetupCommand"/>. Declares no rules, for the reason
/// <c>CompleteSetupValidator</c> declares none: the token is optional, and whether it matches is
/// the shared write's answer, not a field rule.
/// </summary>
public sealed class ClaimSetupValidator : LocalizedValidator<ClaimSetupCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ClaimSetupValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
