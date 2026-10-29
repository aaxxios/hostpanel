using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Reports.Queries.NewCustomers;

/// <summary>
/// Validator for <see cref="NewCustomersQuery"/>. Its one input is a year, which every value of
/// (including <see langword="null"/>) is valid, so no rule is declared: decided, not forgotten.
/// </summary>
public sealed class NewCustomersValidator : LocalizedValidator<NewCustomersQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public NewCustomersValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
