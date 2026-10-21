using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Locations.Queries.ListActiveLocations;

/// <summary>Validates <see cref="ListActiveLocationsQuery"/> before it reaches the handler. The query carries no input, so there are no rules.</summary>
public sealed class ListActiveLocationsValidator : LocalizedValidator<ListActiveLocationsQuery>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ListActiveLocationsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
