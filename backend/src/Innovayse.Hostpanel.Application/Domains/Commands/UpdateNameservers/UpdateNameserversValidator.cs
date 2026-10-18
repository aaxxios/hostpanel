using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Domains.Commands.UpdateNameservers;

/// <summary>Validates <see cref="UpdateNameserversCommand"/> inputs before the handler executes.</summary>
public sealed class UpdateNameserversValidator : LocalizedValidator<UpdateNameserversCommand>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public UpdateNameserversValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.DomainId).GreaterThan(0).WithMessage(L["DomainIdRequired"]);
        RuleFor(x => x.Nameservers)
            .Must(ns => ns is not null && ns.Count >= 2)
            .WithMessage(L["DomainNameserversMinimumTwo"]);
    }
}
