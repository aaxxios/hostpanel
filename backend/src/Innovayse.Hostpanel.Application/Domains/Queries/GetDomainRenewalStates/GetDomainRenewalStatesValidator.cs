using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Domains.Queries.GetDomainRenewalStates;

/// <summary>Validates <see cref="GetDomainRenewalStatesQuery"/>: every id must be a real id.</summary>
public sealed class GetDomainRenewalStatesValidator : LocalizedValidator<GetDomainRenewalStatesQuery>
{
    /// <summary>Initializes the rules.</summary>
    /// <param name="localizer">Validator sentences.</param>
    public GetDomainRenewalStatesValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleForEach(x => x.DomainIds).MustBePositiveId(L["DomainIdRequired"]);
    }
}
