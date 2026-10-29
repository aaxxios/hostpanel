using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Queries.ListTickets;

/// <summary>Validator for <see cref="ListTicketsQuery"/>.</summary>
/// <remarks>
/// Page and page size are clamped by the handler and the search term is free text that simply
/// matches nothing. A <c>Status</c> outside the enum is refused, so a bad filter is a 400
/// instead of silently listing every ticket.
/// </remarks>
public sealed class ListTicketsValidator : LocalizedValidator<ListTicketsQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ListTicketsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage(L["TicketStatusInvalid"]);
    }
}
