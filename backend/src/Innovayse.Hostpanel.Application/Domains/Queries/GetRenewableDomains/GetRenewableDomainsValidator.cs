using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Domains.Queries.GetRenewableDomains;

/// <summary>Validates <see cref="GetRenewableDomainsQuery"/>: a named client must be a real id.</summary>
public sealed class GetRenewableDomainsValidator : LocalizedValidator<GetRenewableDomainsQuery>
{
    /// <summary>Initializes the rules.</summary>
    /// <param name="localizer">Validator sentences.</param>
    public GetRenewableDomainsValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        When(x => x.ClientId is not null, () =>
        {
            RuleFor(x => x.ClientId!.Value).MustBePositiveId(L["ClientIdInvalid"]);
        });
    }
}
