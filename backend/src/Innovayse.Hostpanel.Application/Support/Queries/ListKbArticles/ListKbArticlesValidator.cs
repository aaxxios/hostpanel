using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Queries.ListKbArticles;

/// <summary>Validator for <see cref="ListKbArticlesQuery"/>.</summary>
/// <remarks>No rule: a boolean flag has no invalid shape. Kept so the pattern is uniform.</remarks>
public sealed class ListKbArticlesValidator : LocalizedValidator<ListKbArticlesQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ListKbArticlesValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
