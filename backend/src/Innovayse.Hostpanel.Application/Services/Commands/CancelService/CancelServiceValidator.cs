using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Innovayse.Hostpanel.Domain.Services.Enums;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Services.Commands.CancelService;
/// <summary>Validates <see cref="CancelServiceCommand"/> before the handler executes.</summary>
public sealed class CancelServiceValidator : LocalizedValidator<CancelServiceCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public CancelServiceValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ServiceId).MustBePositiveId(L["ServiceIdInvalid"]);
        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(t => t is nameof(CancellationType.Immediate) or nameof(CancellationType.EndOfBillingPeriod))
            .WithMessage(L["CancellationTypeInvalid"]);
    }
}
