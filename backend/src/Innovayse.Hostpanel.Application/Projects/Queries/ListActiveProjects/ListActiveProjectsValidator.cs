using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Projects.Queries.ListActiveProjects;

/// <summary>Validates <see cref="ListActiveProjectsQuery"/> before it reaches the handler. The query carries no input, so there are no rules.</summary>
public sealed class ListActiveProjectsValidator : LocalizedValidator<ListActiveProjectsQuery>
{
    /// <summary>
    ///     Initializes validator with localization service.
    /// </summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ListActiveProjectsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
