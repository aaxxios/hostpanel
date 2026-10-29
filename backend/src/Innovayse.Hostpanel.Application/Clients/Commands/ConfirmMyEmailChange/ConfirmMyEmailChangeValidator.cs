using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Clients.Commands.ConfirmMyEmailChange;

/// <summary>
/// Validates <see cref="ConfirmMyEmailChangeCommand"/>: both halves of the link are present. Whether
/// the token matches is the handler's answer.
/// </summary>
public sealed class ConfirmMyEmailChangeValidator : LocalizedValidator<ConfirmMyEmailChangeCommand>
{
    /// <summary>Initializes the validator with the localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ConfirmMyEmailChangeValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage(L["ClientNewEmailInvalid"]);
        RuleFor(x => x.Token).NotEmpty().WithMessage(L["ClientEmailChangeTokenRequired"]);
    }
}
