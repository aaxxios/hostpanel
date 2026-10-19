using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Partners.Queries.ListActivePartners;

/// <summary>Validates <see cref="ListActivePartnersQuery"/> before it reaches the handler. The query carries no input, so there are no rules.</summary>
public sealed class ListActivePartnersValidator : LocalizedValidator<ListActivePartnersQuery>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ListActivePartnersValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
