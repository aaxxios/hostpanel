using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Auth.Commands.RequestPasswordReset;

/// <summary>
/// Validator for <see cref="RequestPasswordResetCommand"/>. Declares no rules: a malformed or unknown
/// address is simply an address no account holds, and the handler answers it like any other so the
/// outcome never tells a caller whether an account exists.
/// </summary>
public sealed class RequestPasswordResetValidator : LocalizedValidator<RequestPasswordResetCommand>
{
    /// <summary>Initializes the validator with the localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public RequestPasswordResetValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
