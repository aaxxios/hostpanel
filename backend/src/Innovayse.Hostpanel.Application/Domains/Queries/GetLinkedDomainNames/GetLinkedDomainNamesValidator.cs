using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Domains.Queries.GetLinkedDomainNames;

/// <summary>Validates <see cref="GetLinkedDomainNamesQuery"/> before it reaches the handler.</summary>
public sealed class GetLinkedDomainNamesValidator : LocalizedValidator<GetLinkedDomainNamesQuery>
{
    /// <summary>Initializes the validator with the localized messages.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public GetLinkedDomainNamesValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleForEach(x => x.ServiceIds).GreaterThan(0).WithMessage(L["ServiceIdInvalid"]);
    }
}
