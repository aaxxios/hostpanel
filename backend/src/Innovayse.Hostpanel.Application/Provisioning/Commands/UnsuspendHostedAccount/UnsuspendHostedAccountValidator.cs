using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Provisioning.Commands.UnsuspendHostedAccount;

/// <summary>Validates <see cref="UnsuspendHostedAccountCommand"/> before the handler executes.</summary>
public sealed class UnsuspendHostedAccountValidator : LocalizedValidator<UnsuspendHostedAccountCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UnsuspendHostedAccountValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ServiceId).MustBePositiveId(L["ServiceIdInvalid"]);
    }
}
