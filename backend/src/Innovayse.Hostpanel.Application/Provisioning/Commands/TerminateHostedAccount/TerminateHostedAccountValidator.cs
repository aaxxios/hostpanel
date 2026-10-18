using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Provisioning.Commands.TerminateHostedAccount;

/// <summary>Validates <see cref="TerminateHostedAccountCommand"/> before the handler executes.</summary>
public sealed class TerminateHostedAccountValidator : LocalizedValidator<TerminateHostedAccountCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public TerminateHostedAccountValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ServiceId).MustBePositiveId(L["ServiceIdInvalid"]);
        RuleFor(x => x.Reason).NotEmpty().WithMessage(L["TerminateReasonRequired"]);
    }
}
