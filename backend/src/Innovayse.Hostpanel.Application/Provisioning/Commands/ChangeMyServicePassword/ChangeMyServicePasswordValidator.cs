using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Provisioning.Commands.ChangeMyServicePassword;

/// <summary>Validates <see cref="ChangeMyServicePasswordCommand"/> before it reaches the handler.</summary>
public sealed class ChangeMyServicePasswordValidator : LocalizedValidator<ChangeMyServicePasswordCommand>
{
    /// <summary>Initializes the validator with the localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ChangeMyServicePasswordValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ServiceId).MustBePositiveId(L["ServiceIdInvalid"]);
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage(L["ServicePasswordRequired"])
            .MinimumLength(8).WithMessage(L["ServicePasswordInvalid"]);
    }
}
