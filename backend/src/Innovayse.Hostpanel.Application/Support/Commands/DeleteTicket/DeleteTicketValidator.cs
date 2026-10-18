using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Commands.DeleteTicket;

/// <summary>Validator for <see cref="DeleteTicketCommand"/>.</summary>
public sealed class DeleteTicketValidator : LocalizedValidator<DeleteTicketCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public DeleteTicketValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.TicketId)
            .GreaterThan(0)
            .WithMessage(L["TicketIdInvalid"]);
    }
}
