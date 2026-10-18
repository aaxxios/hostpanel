using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Commands.ReplyToMyTicket;

/// <summary>Validator for <see cref="ReplyToMyTicketCommand"/>.</summary>
public sealed class ReplyToMyTicketValidator : LocalizedValidator<ReplyToMyTicketCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ReplyToMyTicketValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.TicketId)
            .GreaterThan(0)
            .WithMessage(L["TicketIdInvalid"]);

        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage(L["TicketMessageRequired"]);
    }
}
