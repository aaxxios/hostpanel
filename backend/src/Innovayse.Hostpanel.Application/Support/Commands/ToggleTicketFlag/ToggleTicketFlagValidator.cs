using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Support.Commands.ToggleTicketFlag;

/// <summary>Validator for <see cref="ToggleTicketFlagCommand"/>.</summary>
public sealed class ToggleTicketFlagValidator : LocalizedValidator<ToggleTicketFlagCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public ToggleTicketFlagValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.TicketId)
            .GreaterThan(0)
            .WithMessage(L["TicketIdInvalid"]);
    }
}
