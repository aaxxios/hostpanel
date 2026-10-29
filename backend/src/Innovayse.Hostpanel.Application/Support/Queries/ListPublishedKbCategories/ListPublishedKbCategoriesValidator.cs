using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Queries.ListPublishedKbCategories;

/// <summary>Validator for <see cref="ListPublishedKbCategoriesQuery"/>.</summary>
/// <remarks>No rule: the query carries no input. Kept so the pattern is uniform.</remarks>
public sealed class ListPublishedKbCategoriesValidator : LocalizedValidator<ListPublishedKbCategoriesQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ListPublishedKbCategoriesValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
