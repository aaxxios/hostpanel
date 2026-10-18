using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Commands.AssignTicket;

/// <summary>Validator for <see cref="AssignTicketCommand"/>.</summary>
public sealed class AssignTicketValidator : LocalizedValidator<AssignTicketCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public AssignTicketValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.TicketId)
            .GreaterThan(0)
            .WithMessage(L["TicketIdInvalid"]);

        RuleFor(x => x.StaffId)
            .GreaterThan(0)
            .WithMessage(L["TicketStaffIdInvalid"]);
    }
}
