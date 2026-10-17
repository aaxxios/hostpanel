using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Auth.Commands.BackchannelLogout;

/// <summary>Validator for <see cref="BackchannelLogoutCommand"/>: the address must be present and well formed.</summary>
public sealed class BackchannelLogoutValidator : LocalizedValidator<BackchannelLogoutCommand>
{
    /// <summary>Initializes the validator.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public BackchannelLogoutValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256)
            .WithMessage(L["BackchannelLogoutEmailInvalid"]);
    }
}
