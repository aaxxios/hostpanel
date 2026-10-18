using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Reports.Queries.AnnualIncome;

/// <summary>
/// Validator for <see cref="AnnualIncomeQuery"/>. Its one input is an optional year, which every
/// value of (including <see langword="null"/>) is valid, so no rule is declared — decided, not
/// forgotten.
/// </summary>
public sealed class AnnualIncomeValidator : LocalizedValidator<AnnualIncomeQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public AnnualIncomeValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
