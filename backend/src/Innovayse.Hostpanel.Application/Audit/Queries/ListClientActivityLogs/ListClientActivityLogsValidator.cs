using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Audit.Queries.ListClientActivityLogs;

/// <summary>Validates <see cref="ListClientActivityLogsQuery"/>.</summary>
public sealed class ListClientActivityLogsValidator : LocalizedValidator<ListClientActivityLogsQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ListClientActivityLogsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ClientId).MustBePositiveId(L["ActivityLogClientIdInvalid"]);
    }
}
