using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Domains.Commands.UpdateMyDomainNameservers;

/// <summary>Validates <see cref="UpdateMyDomainNameserversCommand"/> before it reaches the handler.</summary>
public sealed class UpdateMyDomainNameserversValidator : LocalizedValidator<UpdateMyDomainNameserversCommand>
{
    /// <summary>Initializes the validator with the localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateMyDomainNameserversValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.DomainId).MustBePositiveId(L["DomainIdRequired"]);
        RuleFor(x => x.Nameservers)
            .Must(ns => ns is not null && ns.Count >= 2)
            .WithMessage(L["DomainNameserversMinimumTwo"]);
    }
}
