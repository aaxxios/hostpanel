using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Commands.AddTicketTag;

/// <summary>Validator for <see cref="AddTicketTagCommand"/>.</summary>
public sealed class AddTicketTagValidator : LocalizedValidator<AddTicketTagCommand>
{
    /// <summary>Longest tag text accepted.</summary>
    private const int MaxTagLength = 64;

    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public AddTicketTagValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.TicketId)
            .GreaterThan(0)
            .WithMessage(L["TicketIdInvalid"]);

        RuleFor(x => x.Tag)
            .NotEmpty()
            .MaximumLength(MaxTagLength)
            .WithMessage(L["TicketTagRequired"]);
    }
}
