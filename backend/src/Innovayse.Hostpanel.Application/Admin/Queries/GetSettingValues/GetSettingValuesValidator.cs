using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Admin.Queries.GetSettingValues;

/// <summary>Validates <see cref="GetSettingValuesQuery"/> before it reaches the handler.</summary>
public sealed class GetSettingValuesValidator : LocalizedValidator<GetSettingValuesQuery>
{
    /// <summary>Initializes the rules.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public GetSettingValuesValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Keys).NotEmpty().WithMessage(L["SettingKeysRequired"]);
        RuleForEach(x => x.Keys).NotEmpty().WithMessage(L["SettingKeysRequired"]);
    }
}
