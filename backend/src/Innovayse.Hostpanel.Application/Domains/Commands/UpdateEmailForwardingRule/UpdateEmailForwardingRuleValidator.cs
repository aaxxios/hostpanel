using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Domains.Commands.UpdateEmailForwardingRule;

/// <summary>
/// Validates <see cref="UpdateEmailForwardingRuleCommand"/> inputs before the handler executes.
/// </summary>
public sealed class UpdateEmailForwardingRuleValidator : LocalizedValidator<UpdateEmailForwardingRuleCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateEmailForwardingRuleValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.DomainId).GreaterThan(0).WithMessage(L["DomainIdRequired"]);
        RuleFor(x => x.RuleId).GreaterThan(0).WithMessage(L["EmailForwardingRuleIdRequired"]);
        RuleFor(x => x.Source).NotEmpty().WithMessage(L["EmailForwardingSourceRequired"]);
        RuleFor(x => x.Destination)
            .NotEmpty().WithMessage(L["EmailForwardingDestinationRequired"])
            .EmailAddress().WithMessage(L["EmailForwardingDestinationInvalid"]);
    }
}
