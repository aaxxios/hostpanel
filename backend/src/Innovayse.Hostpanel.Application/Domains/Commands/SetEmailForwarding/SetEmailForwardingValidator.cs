using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Domains.Commands.SetEmailForwarding;

/// <summary>Validates <see cref="SetEmailForwardingCommand"/> inputs before the handler executes.</summary>
public sealed class SetEmailForwardingValidator : LocalizedValidator<SetEmailForwardingCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public SetEmailForwardingValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.DomainId).GreaterThan(0).WithMessage(L["DomainIdRequired"]);
    }
}
