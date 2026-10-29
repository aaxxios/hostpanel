using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Clients.Commands.StartMyEmailChange;

/// <summary>
/// Validates <see cref="StartMyEmailChangeCommand"/>: a well-formed new address and a password to
/// check. Whether the password is right is the handler's answer.
/// </summary>
public sealed class StartMyEmailChangeValidator : LocalizedValidator<StartMyEmailChangeCommand>
{
    /// <summary>The longest address the identity store keeps.</summary>
    private const int EmailMaxLength = 256;

    /// <summary>Initializes the validator with the localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public StartMyEmailChangeValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.NewEmail)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(EmailMaxLength)
            .WithMessage(L["ClientNewEmailInvalid"]);
        RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage(L["ClientCurrentPasswordRequired"]);
    }
}
