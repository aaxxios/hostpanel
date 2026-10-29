using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Auth.Commands.ForgotPassword;

/// <summary>
/// Validator for <see cref="ForgotPasswordCommand"/>. Declares no rules: a malformed or unknown
/// address is simply an address no account holds, and the answer must not tell a caller whether
/// an account exists.
/// </summary>
public sealed class ForgotPasswordValidator : LocalizedValidator<ForgotPasswordCommand>
{
    /// <summary>Initializes the validator with the localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ForgotPasswordValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
