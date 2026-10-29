using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Services.Queries.GetServiceBillingStates;

/// <summary>Validates <see cref="GetServiceBillingStatesQuery"/>: every id must be a real id.</summary>
public sealed class GetServiceBillingStatesValidator : LocalizedValidator<GetServiceBillingStatesQuery>
{
    /// <summary>Initializes the rules.</summary>
    /// <param name="localizer">Validator sentences.</param>
    public GetServiceBillingStatesValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        RuleForEach(x => x.ServiceIds).MustBePositiveId(L["ServiceIdInvalid"]);
    }
}
