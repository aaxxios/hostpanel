using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Commands.BulkUpdateTickets;

/// <summary>Validator for <see cref="BulkUpdateTicketsCommand"/>.</summary>
public sealed class BulkUpdateTicketsValidator : LocalizedValidator<BulkUpdateTicketsCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public BulkUpdateTicketsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.TicketIds)
            .NotEmpty()
            .WithMessage(L["TicketIdsRequired"]);

        RuleFor(x => x.Action)
            .IsInEnum()
            .WithMessage(L["TicketBulkActionInvalid"]);
    }
}
