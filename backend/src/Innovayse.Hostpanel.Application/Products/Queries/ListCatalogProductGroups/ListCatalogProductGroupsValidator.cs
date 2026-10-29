using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Products.Queries.ListCatalogProductGroups;

/// <summary>
///     Validator for <see cref="ListCatalogProductGroupsQuery"/>.
///     The query carries no input, so no rule is declared: decided, not forgotten.
/// </summary>
public sealed class ListCatalogProductGroupsValidator : LocalizedValidator<ListCatalogProductGroupsQuery>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ListCatalogProductGroupsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
