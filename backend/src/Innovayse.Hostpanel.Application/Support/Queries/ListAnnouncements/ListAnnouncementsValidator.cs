using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Queries.ListAnnouncements;

/// <summary>Validator for <see cref="ListAnnouncementsQuery"/>.</summary>
/// <remarks>
/// No rule: the handler clamps <c>Page</c> to at least 1 and <c>PageSize</c> to [1, 100] itself,
/// so there is no shape of input this validator could refuse that the handler would not already
/// answer sensibly for.
/// </remarks>
public sealed class ListAnnouncementsValidator : LocalizedValidator<ListAnnouncementsQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ListAnnouncementsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
