using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Provisioning.Commands.ChangePassword;

/// <summary>Validates <see cref="ChangePasswordCommand"/> before the handler executes.</summary>
public sealed class ChangePasswordValidator : LocalizedValidator<ChangePasswordCommand>
{
    /// <summary>Shortest password the platform accepts, matching the setup wizard's own rule.</summary>
    private const int MinimumPasswordLength = 8;

    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ChangePasswordValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ServiceId).MustBePositiveId(L["ServiceIdInvalid"]);

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage(L["ServicePasswordRequired"])
            .MinimumLength(MinimumPasswordLength).WithMessage(L["ServicePasswordInvalid"]);
    }
}
