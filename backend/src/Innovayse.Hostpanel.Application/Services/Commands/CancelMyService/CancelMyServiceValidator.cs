using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Services.Commands.CancelMyService;

/// <summary>Validates <see cref="CancelMyServiceCommand"/> before the handler executes.</summary>
public sealed class CancelMyServiceValidator : LocalizedValidator<CancelMyServiceCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public CancelMyServiceValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ServiceId).MustBePositiveId(L["ServiceIdInvalid"]);
        RuleFor(x => x.Type).IsInEnum().WithMessage(L["CancellationTypeInvalid"]);
    }
}
