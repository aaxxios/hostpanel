using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Innovayse.Hostpanel.Domain.Support;
using Innovayse.Hostpanel.Domain.Support.Enums;
using Microsoft.Extensions.Localization;
namespace Innovayse.Hostpanel.Application.Support.Commands.AdminCreateTicket;

/// <summary>Validator for <see cref="AdminCreateTicketCommand"/>.</summary>
public sealed class AdminCreateTicketValidator : LocalizedValidator<AdminCreateTicketCommand>
{
    /// <summary>Longest subject line accepted.</summary>
    private const int MaxSubjectLength = 255;

    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public AdminCreateTicketValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.ClientId)
            .GreaterThan(0)
            .WithMessage(L["TicketClientIdInvalid"]);

        RuleFor(x => x.DepartmentId)
            .NotNull()
            .WithMessage(L["TicketDepartmentRequired"]);

        RuleFor(x => x.DepartmentId)
            .GreaterThan(0)
            .When(x => x.DepartmentId.HasValue)
            .WithMessage(L["TicketDepartmentIdInvalid"]);

        RuleFor(x => x.Subject)
            .NotEmpty()
            .MaximumLength(MaxSubjectLength)
            .WithMessage(L["TicketSubjectRequired"]);

        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage(L["TicketMessageRequired"]);

        RuleFor(x => x.Priority)
            .NotEmpty()
            .Must(p => Enum.TryParse<TicketPriority>(p, ignoreCase: true, out _))
            .WithMessage(L["TicketPriorityInvalid"]);
    }
}
