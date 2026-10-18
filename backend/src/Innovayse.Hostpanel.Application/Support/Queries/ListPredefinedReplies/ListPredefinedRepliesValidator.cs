using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Queries.ListPredefinedReplies;

/// <summary>
/// Validator for <see cref="ListPredefinedRepliesQuery"/>. Its one input is an optional category
/// filter, which every value of (including <see langword="null"/>) is valid, so no rule is
/// declared — decided, not forgotten.
/// </summary>
public sealed class ListPredefinedRepliesValidator : LocalizedValidator<ListPredefinedRepliesQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ListPredefinedRepliesValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
