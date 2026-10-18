using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Queries.ListClientTickets;

/// <summary>
///     Validator for ListClientTicketsQuery. Page and page size are clamped to a sane window by
///     the handler and search is a free-text filter that simply matches nothing when it names no
///     ticket, so only the client id gets a rule.
/// </summary>
public sealed class ListClientTicketsValidator : LocalizedValidator<ListClientTicketsQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ListClientTicketsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ClientId)
            .GreaterThan(0)
            .WithMessage(L["TicketClientIdInvalid"]);
    }
}
