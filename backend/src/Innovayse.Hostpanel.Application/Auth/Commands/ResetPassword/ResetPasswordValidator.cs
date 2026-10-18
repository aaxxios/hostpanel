using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Auth.Commands.ResetPassword;

/// <summary>
/// Validator for <see cref="ResetPasswordCommand"/>. Declares no rules: a malformed email or token
/// is simply a token that does not match, answered by the handler as
/// <c>TOKEN_INVALID_OR_EXPIRED</c>, and the password policy belongs to the identity store.
/// The reset endpoint behaved this way before it had a command; that is kept.
/// </summary>
public sealed class ResetPasswordValidator : LocalizedValidator<ResetPasswordCommand>
{
    /// <summary>Initializes the validator with the localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ResetPasswordValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
