using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Auth.Commands.ResendVerification;

/// <summary>
/// Validator for <see cref="ResendVerificationCommand"/>. Declares no rules: the command carries
/// no fields, the caller's identity comes from the credential.
/// </summary>
public sealed class ResendVerificationValidator : LocalizedValidator<ResendVerificationCommand>
{
    /// <summary>Initializes the validator with the localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ResendVerificationValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
