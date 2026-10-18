using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Domains.Commands.SetWhoisPrivacy;

/// <summary>Validates <see cref="SetWhoisPrivacyCommand"/> inputs before the handler executes.</summary>
public sealed class SetWhoisPrivacyValidator : LocalizedValidator<SetWhoisPrivacyCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public SetWhoisPrivacyValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.DomainId).GreaterThan(0).WithMessage(L["DomainIdRequired"]);
    }
}
