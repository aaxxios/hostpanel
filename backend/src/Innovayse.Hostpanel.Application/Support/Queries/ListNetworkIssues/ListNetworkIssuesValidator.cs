using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Queries.ListNetworkIssues;

/// <summary>Validator for <see cref="ListNetworkIssuesQuery"/>.</summary>
/// <remarks>
/// No rule: the handler clamps <c>Page</c>/<c>PageSize</c> itself and silently ignores a
/// <c>Status</c> value that does not parse, treating it the same as no filter at all.
/// </remarks>
public sealed class ListNetworkIssuesValidator : LocalizedValidator<ListNetworkIssuesQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ListNetworkIssuesValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
