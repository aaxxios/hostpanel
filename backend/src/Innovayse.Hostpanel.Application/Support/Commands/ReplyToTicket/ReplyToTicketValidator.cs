using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Commands.ReplyToTicket;

/// <summary>Validator for <see cref="ReplyToTicketCommand"/>.</summary>
public sealed class ReplyToTicketValidator : LocalizedValidator<ReplyToTicketCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ReplyToTicketValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
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
