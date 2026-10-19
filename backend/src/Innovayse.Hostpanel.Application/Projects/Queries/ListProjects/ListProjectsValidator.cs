using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Projects.Queries.ListProjects;

/// <summary>Validates <see cref="ListProjectsQuery"/> before it reaches the handler. The query carries no input, so there are no rules.</summary>
public sealed class ListProjectsValidator : LocalizedValidator<ListProjectsQuery>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ListProjectsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
