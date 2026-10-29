using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Auth.Commands.ConfirmEmail;

/// <summary>
/// Validator for <see cref="ConfirmEmailCommand"/>. Declares no rules: a malformed email or token is
/// simply a token that does not match, answered by the handler as <c>TOKEN_INVALID_OR_EXPIRED</c>.
/// </summary>
public sealed class ConfirmEmailValidator : LocalizedValidator<ConfirmEmailCommand>
{
    /// <summary>Initializes the validator with the localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ConfirmEmailValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
