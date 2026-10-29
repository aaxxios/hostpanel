using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Services.Commands.UpdateServiceAutomation;

/// <summary>Validates <see cref="UpdateServiceAutomationCommand"/> before the handler executes.</summary>
public sealed class UpdateServiceAutomationValidator : LocalizedValidator<UpdateServiceAutomationCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateServiceAutomationValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ServiceId).MustBePositiveId(L["ServiceIdInvalid"]);
        RuleFor(x => x.SuspendUntil).Null().WithMessage(L["ServiceSuspendUntilWithoutOverride"])
            .When(x => !x.OverrideAutoSuspend);
        RuleFor(x => x.AutoTerminateReason).Null().WithMessage(L["ServiceAutoTerminateReasonWithoutFlag"])
            .When(x => !x.AutoTerminateEndOfCycle);
        RuleFor(x => x.AutoTerminateReason).MaximumLength(2000).WithMessage(L["ServiceAutoTerminateReasonInvalid"])
            .When(x => x.AutoTerminateReason is not null);
    }
}
