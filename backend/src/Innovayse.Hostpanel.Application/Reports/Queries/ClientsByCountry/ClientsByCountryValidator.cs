using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Reports.Queries.ClientsByCountry;

/// <summary>
/// Validator for <see cref="ClientsByCountryQuery"/>. The query carries no input, so it declares
/// no rules; the absence was decided, not forgotten.
/// </summary>
public sealed class ClientsByCountryValidator : LocalizedValidator<ClientsByCountryQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ClientsByCountryValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
