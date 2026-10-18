using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Queries.ListKbCategories;

/// <summary>Validator for <see cref="ListKbCategoriesQuery"/>.</summary>
/// <remarks>No rule: the query carries no input to validate. Kept so the pattern is uniform.</remarks>
public sealed class ListKbCategoriesValidator : LocalizedValidator<ListKbCategoriesQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ListKbCategoriesValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
