using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Auth.Commands.SignOut;

/// <summary>
/// Validator for <see cref="SignOutCommand"/>. The command carries no input, so there is no rule to
/// declare; the validator exists so every command has one, and a rule added later lands here.
/// </summary>
public sealed class SignOutValidator : LocalizedValidator<SignOutCommand>
{
    /// <summary>Initializes the validator with the localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public SignOutValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
