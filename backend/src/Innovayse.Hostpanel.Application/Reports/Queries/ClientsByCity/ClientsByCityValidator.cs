using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Reports.Queries.ClientsByCity;

/// <summary>
/// Validator for <see cref="ClientsByCityQuery"/>. The query carries no input, so it declares no
/// rules; the absence was decided, not forgotten.
/// </summary>
public sealed class ClientsByCityValidator : LocalizedValidator<ClientsByCityQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ClientsByCityValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
