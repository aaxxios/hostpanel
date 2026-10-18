using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Queries.ListPublishedAnnouncements;

/// <summary>Validator for <see cref="ListPublishedAnnouncementsQuery"/>.</summary>
/// <remarks>
/// No rule: the handler clamps <c>Page</c> to at least 1 and <c>PageSize</c> to [1, 100] itself.
/// </remarks>
public sealed class ListPublishedAnnouncementsValidator : LocalizedValidator<ListPublishedAnnouncementsQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ListPublishedAnnouncementsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
