using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Queries.ListTickets;

/// <summary>
///     Validator for ListTicketsQuery.
///     Page and page size are clamped to a sane window by the handler and status/search are free
///     text filters that simply match nothing when they name no ticket, so no rule is declared —
///     decided, not forgotten.
/// </summary>
public sealed class ListTicketsValidator : LocalizedValidator<ListTicketsQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ListTicketsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
