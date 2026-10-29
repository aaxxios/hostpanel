using FluentValidation;
using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Domains.Queries.GetTldRenewPrice;

/// <summary>Validates <see cref="GetTldRenewPriceQuery"/>.</summary>
public sealed class GetTldRenewPriceValidator : LocalizedValidator<GetTldRenewPriceQuery>
{
    /// <summary>Initializes the rules.</summary>
    /// <param name="localizer">Validator sentences.</param>
    public GetTldRenewPriceValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleFor(x => x.DomainName).NotEmpty().WithMessage(L["DomainNameRequired"]);
        RuleFor(x => x.Years).InclusiveBetween(1, 10).WithMessage(L["DomainYearsRange"]);
    }
}
