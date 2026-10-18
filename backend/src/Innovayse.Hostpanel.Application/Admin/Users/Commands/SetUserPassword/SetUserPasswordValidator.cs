using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Admin.Users.Commands.SetUserPassword;

/// <summary>Validates <see cref="SetUserPasswordCommand"/> before it reaches the handler.</summary>
public sealed class SetUserPasswordValidator : LocalizedValidator<SetUserPasswordCommand>
{
    /// <summary>Fewest characters a password set through this action may have.</summary>
    private const int MinimumPasswordLength = 8;

    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public SetUserPasswordValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage(L["UserIdRequired"]);
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(L["UserPasswordRequired"])
            .MinimumLength(MinimumPasswordLength).WithMessage(L["UserPasswordTooShort"]);
    }
}
