using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Domains.Commands.AddMyDomainEmailForwardingRule;

/// <summary>Validates <see cref="AddMyDomainEmailForwardingRuleCommand"/> before it reaches the handler.</summary>
public sealed class AddMyDomainEmailForwardingRuleValidator : LocalizedValidator<AddMyDomainEmailForwardingRuleCommand>
{
    /// <summary>Initializes the validator with the localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public AddMyDomainEmailForwardingRuleValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.DomainId).MustBePositiveId(L["DomainIdRequired"]);
        RuleFor(x => x.Source).NotEmpty().WithMessage(L["EmailForwardingSourceRequired"]);
        RuleFor(x => x.Destination)
            .NotEmpty().WithMessage(L["EmailForwardingDestinationRequired"])
            .EmailAddress().WithMessage(L["EmailForwardingDestinationInvalid"]);
    }
}
