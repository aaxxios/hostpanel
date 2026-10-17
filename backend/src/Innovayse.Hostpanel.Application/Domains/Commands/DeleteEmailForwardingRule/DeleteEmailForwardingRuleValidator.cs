using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Domains.Commands.DeleteEmailForwardingRule;

/// <summary>
/// Validates <see cref="DeleteEmailForwardingRuleCommand"/> inputs before the handler executes.
/// </summary>
public sealed class DeleteEmailForwardingRuleValidator : LocalizedValidator<DeleteEmailForwardingRuleCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public DeleteEmailForwardingRuleValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.DomainId).GreaterThan(0).WithMessage(L["DomainIdRequired"]);
        RuleFor(x => x.RuleId).GreaterThan(0).WithMessage(L["EmailForwardingRuleIdRequired"]);
    }
}
