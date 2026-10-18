using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Auth.Commands.DisableTwoFactor;

/// <summary>
///     Validator for DisableTwoFactorCommand. Checks the shape of a TOTP code; whether it matches
///     the account's secret is the handler's answer.
/// </summary>
public sealed class DisableTwoFactorValidator : LocalizedValidator<DisableTwoFactorCommand>
{
    /// <summary>Digits a TOTP code carries.</summary>
    private const int CodeLength = 6;

    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public DisableTwoFactorValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .Length(CodeLength)
            .Must(code => code.All(char.IsAsciiDigit))
            .WithMessage(L["TwoFactorCodeInvalid"]);
    }
}
