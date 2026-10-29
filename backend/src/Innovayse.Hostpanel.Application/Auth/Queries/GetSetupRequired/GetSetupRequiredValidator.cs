using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Auth.Queries.GetSetupRequired;

/// <summary>
///     Validates <see cref="GetSetupRequiredQuery"/>. The query carries no input, so there is no rule to
///     declare; the validator exists so every query has one, and a rule added later lands here.
/// </summary>
public sealed class GetSetupRequiredValidator : LocalizedValidator<GetSetupRequiredQuery>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public GetSetupRequiredValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
