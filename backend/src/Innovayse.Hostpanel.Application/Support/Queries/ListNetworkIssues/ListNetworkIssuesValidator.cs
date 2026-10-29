using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Queries.ListNetworkIssues;

/// <summary>Validator for <see cref="ListNetworkIssuesQuery"/>.</summary>
/// <remarks>
/// The handler clamps <c>Page</c>/<c>PageSize</c> itself. A status outside the enum is refused,
/// so a bad filter is a 400 instead of silently listing every issue.
/// </remarks>
public sealed class ListNetworkIssuesValidator : LocalizedValidator<ListNetworkIssuesQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ListNetworkIssuesValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleForEach(x => x.Statuses)
            .IsInEnum()
            .WithMessage(L["NetworkIssueStatusInvalid"]);
    }
}
