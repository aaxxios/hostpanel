using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Queries.ListDownloads;

/// <summary>
/// Validator for <see cref="ListDownloadsQuery"/>. Its one input is an optional category filter,
/// which every value of (including <see langword="null"/>) is valid, so no rule is declared —
/// decided, not forgotten.
/// </summary>
public sealed class ListDownloadsValidator : LocalizedValidator<ListDownloadsQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ListDownloadsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
