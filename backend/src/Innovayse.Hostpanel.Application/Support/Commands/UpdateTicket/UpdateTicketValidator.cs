using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Commands.UpdateTicket;

/// <summary>Validator for <see cref="UpdateTicketCommand"/>.</summary>
public sealed class UpdateTicketValidator : LocalizedValidator<UpdateTicketCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateTicketValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.TicketId)
            .GreaterThan(0)
            .WithMessage(L["TicketIdInvalid"]);

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage(L["TicketStatusInvalid"]);

        RuleFor(x => x.Priority)
            .IsInEnum()
            .WithMessage(L["TicketPriorityInvalid"]);

        RuleFor(x => x.DepartmentId)
            .Must(id => id is null || id > 0)
            .WithMessage(L["TicketDepartmentIdInvalid"]);

        RuleFor(x => x.AssignedToStaffId)
            .Must(id => id is null || id > 0)
            .WithMessage(L["TicketStaffIdInvalid"]);
    }
}
