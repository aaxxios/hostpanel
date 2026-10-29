using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Commands.CreateTicket;

/// <summary>Validator for <see cref="CreateTicketCommand"/>.</summary>
public sealed class CreateTicketValidator : LocalizedValidator<CreateTicketCommand>
{
    /// <summary>Longest subject line accepted.</summary>
    private const int MaxSubjectLength = 255;

    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public CreateTicketValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
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
            .IsInEnum()
            .WithMessage(L["TicketPriorityInvalid"]);
    }
}
