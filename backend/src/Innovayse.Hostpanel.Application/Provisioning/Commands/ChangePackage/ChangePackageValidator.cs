using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Provisioning.Commands.ChangePackage;

/// <summary>Validates <see cref="ChangePackageCommand"/> before the handler executes.</summary>
public sealed class ChangePackageValidator : LocalizedValidator<ChangePackageCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ChangePackageValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ServiceId).MustBePositiveId(L["ServiceIdInvalid"]);
        RuleFor(x => x.NewPackage).NotEmpty().WithMessage(L["ServicePackageRequired"]);
    }
}
