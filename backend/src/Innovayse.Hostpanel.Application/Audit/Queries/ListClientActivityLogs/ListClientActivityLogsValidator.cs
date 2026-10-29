using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Audit.Queries.ListClientActivityLogs;

/// <summary>
/// Validates <see cref="ListClientActivityLogsQuery"/>. Page and page size carry no rule on purpose:
/// the history tab may omit them, and the handler clamps both into the shared paging bounds.
/// </summary>
public sealed class ListClientActivityLogsValidator : LocalizedValidator<ListClientActivityLogsQuery>
{
    /// <summary>Longest text a search filter may carry; longer input is not a real search.</summary>
    private const int MaxFilterLength = 200;

    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ListClientActivityLogsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ClientId).MustBePositiveId(L["ActivityLogClientIdInvalid"]);
        RuleFor(x => x.AdminSearch).MaximumLength(MaxFilterLength).WithMessage(L["ActivityLogFilterTooLong"]);
        RuleFor(x => x.Description).MaximumLength(MaxFilterLength).WithMessage(L["ActivityLogFilterTooLong"]);
        RuleFor(x => x.IpAddress).MaximumLength(MaxFilterLength).WithMessage(L["ActivityLogFilterTooLong"]);
    }
}
