using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Services.Commands.ChangeServiceStatus;

/// <summary>Validates <see cref="ChangeServiceStatusCommand"/> before the handler executes.</summary>
public sealed class ChangeServiceStatusValidator : LocalizedValidator<ChangeServiceStatusCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ChangeServiceStatusValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ServiceId).MustBePositiveId(L["ServiceIdInvalid"]);
        RuleFor(x => x.Status).IsInEnum().WithMessage(L["ServiceStatusInvalid"]);
    }
}
