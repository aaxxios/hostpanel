using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Locations.Queries.ListLocations;

/// <summary>Validates <see cref="ListLocationsQuery"/> before it reaches the handler. The query carries no input, so there are no rules.</summary>
public sealed class ListLocationsValidator : LocalizedValidator<ListLocationsQuery>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ListLocationsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
