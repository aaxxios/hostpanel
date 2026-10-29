using Innovayse.Hostpanel.Application.Common.Validators;
using Innovayse.Hostpanel.Application.Resources;
using Microsoft.Extensions.Localization;

namespace Innovayse.Hostpanel.Application.Services.Queries.ListUndatedRenewingServices;

/// <summary>Validates <see cref="ListUndatedRenewingServicesQuery"/>: no rules, the query carries no input.</summary>
public sealed class ListUndatedRenewingServicesValidator : LocalizedValidator<ListUndatedRenewingServicesQuery>
{
    /// <summary>Initializes the validator.</summary>
    /// <param name="localizer">Validator sentences.</param>
    public ListUndatedRenewingServicesValidator(IStringLocalizer<ApplicationValidationMessages> localizer)
        : base(localizer)
    {
    }
}
