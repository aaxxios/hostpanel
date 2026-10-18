using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Domains.Queries.CheckDomainAvailability;

/// <summary>Validates <see cref="CheckDomainAvailabilityQuery"/> inputs before the handler executes.</summary>
public sealed class CheckDomainAvailabilityValidator : LocalizedValidator<CheckDomainAvailabilityQuery>
{
    /// <summary>Initializes validator with localization service.</summary>
    /// <param name="localizer">String localizer for validation messages.</param>
    public CheckDomainAvailabilityValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.Domain).NotEmpty().WithMessage(L["DomainNameRequired"]);
    }
}
