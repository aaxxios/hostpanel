using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Partners.Queries.ListPartners;

/// <summary>Validates <see cref="ListPartnersQuery"/> before it reaches the handler. The query carries no input, so there are no rules.</summary>
public sealed class ListPartnersValidator : LocalizedValidator<ListPartnersQuery>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ListPartnersValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
