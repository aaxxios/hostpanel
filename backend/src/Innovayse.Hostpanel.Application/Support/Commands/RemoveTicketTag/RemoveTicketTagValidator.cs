using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Commands.RemoveTicketTag;

/// <summary>Validator for <see cref="RemoveTicketTagCommand"/>.</summary>
public sealed class RemoveTicketTagValidator : LocalizedValidator<RemoveTicketTagCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public RemoveTicketTagValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.TicketId)
            .GreaterThan(0)
            .WithMessage(L["TicketIdInvalid"]);

        RuleFor(x => x.Tag)
            .NotEmpty()
            .WithMessage(L["TicketTagRequired"]);
    }
}
