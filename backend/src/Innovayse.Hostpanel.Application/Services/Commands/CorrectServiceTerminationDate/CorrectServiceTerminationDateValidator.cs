using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Services.Commands.CorrectServiceTerminationDate;

/// <summary>Validates <see cref="CorrectServiceTerminationDateCommand"/> before the handler executes.</summary>
public sealed class CorrectServiceTerminationDateValidator : LocalizedValidator<CorrectServiceTerminationDateCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public CorrectServiceTerminationDateValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ServiceId).MustBePositiveId(L["ServiceIdInvalid"]);
        RuleFor(x => x.TerminatedAt).NotEmpty().WithMessage(L["ServiceTerminatedAtRequired"]);
    }
}
