using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Reports.Queries.GetIncomeForecast;

/// <summary>
/// Validator for <see cref="GetIncomeForecastQuery"/>. Its one input is an optional year, which
/// every value of (including <see langword="null"/>) is valid, so no rule is declared — decided,
/// not forgotten.
/// </summary>
public sealed class GetIncomeForecastValidator : LocalizedValidator<GetIncomeForecastQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public GetIncomeForecastValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
