using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Provisioning.Commands.SuspendHostedAccount;

/// <summary>Validates <see cref="SuspendHostedAccountCommand"/> before the handler executes.</summary>
public sealed class SuspendHostedAccountValidator : LocalizedValidator<SuspendHostedAccountCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public SuspendHostedAccountValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ServiceId).MustBePositiveId(L["ServiceIdInvalid"]);
        RuleFor(x => x.Reason).NotEmpty().WithMessage(L["SuspendReasonRequired"]);
    }
}
