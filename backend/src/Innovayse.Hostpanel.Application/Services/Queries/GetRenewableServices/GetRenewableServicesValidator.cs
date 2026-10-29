using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Services.Queries.GetRenewableServices;

/// <summary>Validates <see cref="GetRenewableServicesQuery"/>: a named client must be a real id.</summary>
public sealed class GetRenewableServicesValidator : LocalizedValidator<GetRenewableServicesQuery>
{
    /// <summary>Initializes the rules.</summary>
    /// <param name="localizer">Validator sentences.</param>
    public GetRenewableServicesValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
        When(x => x.ClientId is not null, () =>
        {
            RuleFor(x => x.ClientId!.Value).MustBePositiveId(L["ClientIdInvalid"]);
        });
    }
}
