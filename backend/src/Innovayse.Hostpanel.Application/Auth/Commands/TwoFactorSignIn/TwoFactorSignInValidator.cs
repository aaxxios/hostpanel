using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Auth.Commands.TwoFactorSignIn;

/// <summary>
/// Validator for <see cref="TwoFactorSignInCommand"/>. Declares no rules: a missing, empty or
/// unreadable pending token and a missing code are refusals the handler answers with their own
/// sign-in codes (401), the same as a wrong one, so the answer never says which part was off.
/// </summary>
public sealed class TwoFactorSignInValidator : LocalizedValidator<TwoFactorSignInCommand>
{
    /// <summary>Initializes the validator with the localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public TwoFactorSignInValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
